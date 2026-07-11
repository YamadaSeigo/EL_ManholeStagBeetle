using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Animations;

public class StagBeetleGenerator : MonoBehaviour
{
    [SerializeField] private Transform parent;
    
    [Header("Scan")]
    [SerializeField] private ImageUploader imageUploader;
    
    [Header("Bone")] 
    [SerializeField] private Transform Body;
    [SerializeField] private Transform Head;
    [SerializeField] private Transform HeadEnd;
    [SerializeField] private Transform Hips01;
    [SerializeField] private Transform Hips02;
    [SerializeField] private Transform HipEnd;
    [SerializeField] private GameObject LegPrefab;
    
    [Header("Material")]
    [SerializeField] private Material material;
    
    [SerializeField]
    private ScanResultData param;

    [Header("Options")]
    [SerializeField] private Vector3 legCenter;
    [SerializeField] private float bodyScaleY = 0.5f;
    [SerializeField] private float lenBodyScaleZ = 2.5f;
    [SerializeField, Range(0,1)] private float BodyAspectX = 0.5f;

    void Start()
    {
        if (imageUploader != null)
        {
            imageUploader.OnScanResult += Execute;
        }
    }

    void OnDestroy()
    {
        if (imageUploader != null)
        {
            imageUploader.OnScanResult -= Execute;
        }
    }

    void Execute(ScanResultData result)
    {
        param = result;
        SetColor();
        DeformationBody();
        GenerateLegs();
    }
    
    [Button]
    void DeformationBody()
    {
        float length = bodyScaleY * param.body_len;

        float scaleXY = Mathf.Sqrt(param.weight / param.body_len);
        Vector3 newScale = new Vector3(scaleXY * BodyAspectX, length, scaleXY * (1.0f - BodyAspectX));
        Body.localScale = newScale;
        Hips01.localScale = newScale;
    }

    [Button]
    void SetColor()
    {
        var newMat = new Material(material);
        Color newColor = newMat.color;
        // 修正: a ではなく r に代入するように変更
        newColor.r = param.color.r / 255.0f;
        newColor.g = param.color.g / 255.0f;
        newColor.b = param.color.b / 255.0f;
        newColor.a = 1.0f; // 必要に応じてアルファ値を設定

        newMat.color = newColor;
        
        var renderers = parent.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material = newMat;
        }
    }
    
    [Button]
    void GenerateLegs()
    {
        if (parent == null)
        {
            Debug.LogWarning("parentがnullです");
            return;
        }
        
        var legsTransform = parent.Find("Legs");
        if (legsTransform != null)
        {
            DestroyImmediate(legsTransform.gameObject);
        }

        GameObject legsGameObject = new GameObject("Legs");
        legsGameObject.transform.SetParent(parent.transform);
        legsGameObject.transform.localPosition = legCenter;
        
        float bodyDistance = (HeadEnd.transform.position - HipEnd.transform.position).magnitude;
        
        // 体のスケール計算（DeformationBodyと同じロジック）
        float length = bodyScaleY * param.body_len;
        float scaleXY = Mathf.Sqrt(param.weight / param.body_len);
        
        // 【修正1】足の展開幅（Z軸）を、体の「厚み」ではなく「長さ(length)」に比例させる
        float lengthZ = length * lenBodyScaleZ; 
        
        // 【修正2】体の太さに応じて足を左右（X軸）に広げるためのオフセット値
        // ※ 0.5f の係数はお使いのモデルに合わせて適宜調整してください（0にすると元の一直線になります）
        float bodyWidth = scaleXY * BodyAspectX;
        float offsetX = bodyWidth * 0.5f; 
        
        float segZ = lengthZ / (float)param.leg_pairs;
        float startZ = segZ * param.leg_pairs * -0.5f + segZ * 0.5f;
        
        for (int i = 0; i < param.leg_pairs; ++i)
        {
            float offsetZ = startZ + segZ * i;
            
            // 左足の配置（X座標をマイナス方向へオフセット）
            Vector3 offsetLeft = new Vector3(-offsetX, 0, offsetZ);
            var legLeft = Instantiate(LegPrefab, legsGameObject.transform);
            legLeft.transform.localPosition = offsetLeft;
            AttackParentConstraint(bodyDistance, legLeft.transform, Vector3.zero);

            // 右足の配置（X座標をプラス方向へオフセット）
            Vector3 offsetRight = new Vector3(offsetX, 0, offsetZ);
            var legRight = Instantiate(LegPrefab, legsGameObject.transform);
            legRight.transform.localPosition = offsetRight;
            AttackParentConstraint(bodyDistance, legRight.transform, new Vector3(0, 180, 0));
        }
    }

    // 修正: 引数の offset を削除し、target.position から直接距離を求めるように変更
    void AttackParentConstraint(float bodyDistance, Transform target, Vector3 rot)
    {
        float dis = Vector3.Distance(HipEnd.position, target.position);
        float t = dis / bodyDistance;
        
        var constraint = target.gameObject.AddComponent<ParentConstraint>();
        
        Transform targetBone = HipEnd;
        if (t > 0.8f)
        {
            targetBone = Head;
        }
        else if (t > 0.5f)
        {
            targetBone = Body;
        }
        else if (t > 0.2f)
        {
            targetBone = Hips01;
        }
        else if (t > 0.0f)
        {
            targetBone = Hips02;
        }
        
        // 追従先（Source）の設定を作成
        ConstraintSource source = new ConstraintSource();
        source.sourceTransform = targetBone;
        source.weight = 1.0f; // 追従の強さ（1.0で完全同期）

        constraint.AddSource(source);

        // 修正: offset ではなく、すでに正しく配置された target.position を使用して計算
        Vector3 worldDistance = target.position - targetBone.position;

        // 2. ターゲットボーンの「回転（向き）」の逆クォータニオンを取得
        Quaternion boneRotationInverse = Quaternion.Inverse(targetBone.rotation);

        // 3. 世界座標の距離を、ボーンの向きに合わせて回転させる（Scaleを無視してローカル化）
        Vector3 localPosWithoutScale = boneRotationInverse * worldDistance;

        // インデックス0（1つ目のSource）に対してOffsetを適用
        constraint.SetTranslationOffset(0, localPosWithoutScale);
        constraint.SetRotationOffset(0, rot);

        // コンストコンポーネントを有効化
        constraint.constraintActive = true;
    }
}
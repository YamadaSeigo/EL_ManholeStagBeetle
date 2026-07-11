using System;
using System.Collections;
using NaughtyAttributes;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageUploader : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject failedPanel;
    [SerializeField] private GameObject successPanel;
    
    [Header("履歴表示(Scroll View)設定")]
    [Tooltip("Scroll View の中にある Content オブジェクト")]
    public Transform historyContentArea; 
    [Tooltip("1件分のテキスト(UI)のプレハブ")]
    public GameObject historyItemPrefab; 
    
    [Header("Camera App")]
    [SerializeField] private WebGLCameraApp cameraApp; // 自分の環境に合わせてください
    
    [Header("プレイヤー設定")] 
    [Tooltip("画面に配置した InputField をここにセットしてください")]
    public TMP_InputField nameInputField;
        
    private string myPlayerName = "名無しプレイヤー";
    private string myUserId;
    
    [Header("デバッグ用")]
    public Texture2D debugImage;
    
    [Header("データ保存用")]
    public ScanResponse lastResponseData;
    
    public Action<ScanResultData> OnScanResult;
    
    private void Start()
    {
        if (PlayerPrefs.HasKey("MyUserId"))
        {
            myUserId = PlayerPrefs.GetString("MyUserId");
        }
        else
        {
            myUserId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("MyUserId", myUserId);
            PlayerPrefs.Save();
            Debug.Log($"新規ユーザーIDを発行しました: {myUserId}");
        }
        
        if (PlayerPrefs.HasKey("SavedPlayerName"))
        {
            myPlayerName = PlayerPrefs.GetString("SavedPlayerName");
            if (nameInputField != null)
            {
                nameInputField.text = myPlayerName;
            }
        }
    }
    
    public void SavePlayerName()
    {
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text))
        {
            myPlayerName = nameInputField.text;
            PlayerPrefs.SetString("SavedPlayerName", myPlayerName);
            PlayerPrefs.Save(); 
        }
    }
    
    [Button("デバッグ画像を送信 (Send Debug Image)")]
    public void SendDebugImage()
    {
        if (debugImage == null)
        {
            Debug.LogError("デバッグ用の画像がセットされていません！");
            return;
        }
        
        Debug.Log("デバッグ画像の送信を開始します...");
        UploadShot(debugImage);
    }
    
    public void UploadShot(Texture2D capturedTexture)
    {
        SavePlayerName(); // 送信前に名前をセーブ
        StartCoroutine(PostImageAndScan(capturedTexture));
    }

    private IEnumerator PostImageAndScan(Texture2D texture)
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        
        // 1. テクスチャをJPGに変換
        byte[] imageBytes = texture.EncodeToJPG();
        

        WWWForm form = new WWWForm();
        form.AddBinaryData("image_file", imageBytes, "upload.jpg", "image/jpeg");
        
        form.AddField("player_name", myPlayerName);
        form.AddField("user_id", myUserId);

        using (UnityWebRequest request = UnityWebRequest.Post(ScannerClient.serverUrl, form))
        {
            yield return request.SendWebRequest();
            
            if (loadingPanel != null) loadingPanel.SetActive(false);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"通信エラー: {request.error}");
            }
            else
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"サーバーからの返答(生データ): {responseText}");
                
                try
                {
                    lastResponseData = JsonUtility.FromJson<ScanResponse>(responseText);
                    
                    if (lastResponseData.scan_result != null && lastResponseData.scan_result.is_succsess == "OK")
                    {
                        Debug.Log("判定OK！後続の処理を行います。");
                        OnScanResult?.Invoke(lastResponseData.scan_result);
                        successPanel.SetActive(true);
                        cameraApp?.CancelCamera(); // 環境に合わせてコメント外してください
                    }
                    else
                    {
                        Debug.Log("判定NO！処理をここで終了します。");
                        failedPanel.SetActive(true);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"JSONの変換に失敗しました: {e.Message}");
                }
            }
        }
    }
    
    [Button("2. みんなのスキャン履歴を取得する")]
    public void FetchHistory()
    {
        StartCoroutine(FetchHistoryRoutine(ScannerClient.serverUrl + "/history"));
    }

    [Button("3. 自分のスキャン履歴だけを取得する")]
    public void FetchMyHistory()
    {
        StartCoroutine(FetchHistoryRoutine(ScannerClient.serverUrl + $"/my_history?user_id={myUserId}"));
    }

    private IEnumerator FetchHistoryRoutine(string targetUrl)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(targetUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"履歴の取得に失敗: {request.error}");
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                
                try
                {
                    HistoryResponse res = JsonUtility.FromJson<HistoryResponse>(jsonResponse);
                    
                    if (res != null && res.history != null && historyContentArea != null)
                    {
                        foreach (Transform child in historyContentArea) Destroy(child.gameObject);

                        foreach (HistoryEntry entry in res.history)
                        {
                            if (historyItemPrefab != null)
                            {
                                GameObject newItem = Instantiate(historyItemPrefab, historyContentArea);
                                Text itemText = newItem.GetComponent<Text>();
                                
                                if (itemText != null)
                                {
                                    string isSuccessStr = entry.result_data.is_succsess == "OK" ? "成功！" : "失敗";
                                    itemText.text = $"[{entry.scan_time}]\n発見者: {entry.player_name}\n結果: {isSuccessStr}";
                                }
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"履歴JSONの解析に失敗しました: {e.Message}");
                }
            }
        }
    }
}

[System.Serializable]
public class ScanResponse
{
    public string status;
    public string message;
    public ScanResultData scan_result; 
}

[System.Serializable]
public class HistoryResponse
{
    public string status;
    public HistoryEntry[] history;
}

[System.Serializable]
public class HistoryEntry
{
    public int id;
    public string player_name;
    public string scan_time;
    public ScanResultData result_data; 
}

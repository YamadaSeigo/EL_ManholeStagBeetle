using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VersionDisplay : MonoBehaviour
{
    // バージョンを表示したいUIのTextコンポーネントをアタッチします
    public TextMeshProUGUI versionText;

    void Start()
    {
        if (versionText != null)
        {
            // Application.version でバージョン文字列（"1.0"など）を取得
            // 見栄えを良くするために頭に "v" などを付けるのがおすすめです
            versionText.text = $"v{Application.version}";
            
            Debug.Log($"現在のビルドバージョン: {Application.version}");
        }
        else
        {
            Debug.LogWarning("VersionTextが設定されていません。");
        }
    }
}
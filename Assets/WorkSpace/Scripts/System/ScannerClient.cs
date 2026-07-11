using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Networking;

public class ScannerClient : MonoBehaviour
{
    // サーバーのURL
    // ※注意: スマホから接続する場合は 127.0.0.1 ではなく、PCのローカルIPに変更する必要があります
    public static string serverUrl = "https://hangup-appetizer-dazzler.ngrok-free.dev/scan";

    [Button]
    public void SendScanRequest()
    {
        StartCoroutine(PostScanCommand());
    }

    
    private IEnumerator PostScanCommand()
    {
        // 1. 送信するデータを作成（curlの -d "path=C:/work/190629_manhole.jpg" に相当）
        WWWForm form = new WWWForm();
        form.AddField("path", "C:/work/190629_manhole.jpg");

        // 2. POSTリクエストを作成（curl -X POST に相当）
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, form))
        {
            // 通信の実行と待機
            yield return www.SendWebRequest();

            // 3. 結果の判定と処理
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                // エラー時の処理
                Debug.LogError($"通信エラーが発生しました: {www.error}");
            }
            else
            {
                // 成功時の処理（サーバーからの返答を取得して表示）
                Debug.Log($"通信成功！\nサーバーからの返答: {www.downloadHandler.text}");
            }
        }
    }
}
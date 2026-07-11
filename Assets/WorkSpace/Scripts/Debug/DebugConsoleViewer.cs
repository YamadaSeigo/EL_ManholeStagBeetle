using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugConsoleViewer : MonoBehaviour
{
    // ログを表示するUIテキストをアタッチします
    public TextMeshProUGUI consoleText; 
    
    // 表示するログの最大行数
    private int maxLines = 15;
    private string logString = "";

    void OnEnable()
    {
        // Unityのログ出力イベントをフック（監視）する
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        // オブジェクトが無効になったら監視を解除する
        Application.logMessageReceived -= HandleLog;
    }

    // ログが出力されるたびに呼ばれる処理
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // ログの種類によって文字色を変える（HTMLタグを使用）
        string color = "white";
        if (type == LogType.Warning) color = "yellow";
        else if (type == LogType.Error || type == LogType.Exception) color = "red";

        // 新しいログを追加
        string newLog = $"<color={color}>{logString}</color>\n";
        this.logString = newLog + this.logString;

        // 行数が多くなりすぎたら古いものを削除（簡易的な処理）
        string[] lines = this.logString.Split('\n');
        if (lines.Length > maxLines)
        {
            this.logString = string.Join("\n", lines, 0, maxLines);
        }

        // UIテキストを更新
        if (consoleText != null)
        {
            consoleText.text = this.logString;
        }
    }
}
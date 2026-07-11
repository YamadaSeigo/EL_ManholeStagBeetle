using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WebGLCameraApp : MonoBehaviour
{
    [Header("UI設定")]
    public RawImage previewDisplay; // 映像を映すRawImageをアタッチ
    public ImageUploader imageUploader; // 前回のアップロード用スクリプトをアタッチ
    
    private WebCamTexture webCamTexture;

    // ▼①「カメラ起動ボタン」のクリックイベントにこのメソッドを登録する
    public void StartCamera()
    {
        StartCoroutine(InitCameraRoutine());
    }

    private IEnumerator InitCameraRoutine()
    {
        // ユーザーにカメラの許可を求める（ブラウザの許可ポップアップが出ます）
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            // スマホの背面カメラを優先して探す
            string deviceName = "";
            foreach (var device in WebCamTexture.devices)
            {
                if (!device.isFrontFacing)
                {
                    deviceName = device.name;
                    break;
                }
            }

            // WebCamTextureを生成（高解像度すぎるとブラウザが重くなるためHD画質程度に設定）
            if (deviceName != "") {
                webCamTexture = new WebCamTexture(deviceName, 1280, 720);
            } else {
                webCamTexture = new WebCamTexture(1280, 720); // 背面が指定できなければデフォルト
            }

            // RawImageにカメラのテクスチャを割り当てて再生
            previewDisplay.texture = webCamTexture;
            webCamTexture.Play();
        }
        else
        {
            Debug.LogError("カメラが許可されませんでした。");
        }
    }

    void Update()
    {
        // スマホを縦に持った時、カメラ映像が横倒しにならないように回転を補正する
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            previewDisplay.rectTransform.localEulerAngles = new Vector3(0, 0, -webCamTexture.videoRotationAngle);
        }
    }

    // ▼②「シャッターボタン」のクリックイベントにこのメソッドを登録する
    public void TakePhotoAndUpload()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            // 1. 現在カメラに映っている映像から新しいTexture2Dを作成
            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
            
            // 2. ピクセルデータをコピーして適用（これが「カシャッ」の瞬間）
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();

            Debug.Log("写真を撮影しました！サーバーへ送信します。");

            // 3. 前回のサーバー送信クラスに撮影した画像を渡す
            if (imageUploader != null)
            {
                imageUploader.UploadShot(photo);
            }
        }
    }
    
    // ▼③「キャンセルボタン」のクリックイベントにこのメソッドを登録する
    public void CancelCamera()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            // 1. カメラのハードウェア動作を停止する
            webCamTexture.Stop();
            
            // 2. 画面に映っている最後の映像（静止画）を消去して透明にする
            previewDisplay.texture = null;

            Debug.Log("カメラをキャンセルし、停止しました。");
            
            // 必要に応じて、ここで「プレビュー画面のUIパネル全体を非表示にする」
            // などの処理を追加すると、より自然なアプリの挙動になります。
            // previewPanel.SetActive(false); 
        }
    }
}
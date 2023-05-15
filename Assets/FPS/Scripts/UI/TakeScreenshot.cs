using System.IO;
using FPS.Scripts.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
#endif

namespace FPS.Scripts.UI
{
    public class TakeScreenshot : MonoBehaviour
    {
        [Tooltip("Root of the screenshot panel in the menu")]
        public GameObject ScreenshotPanel;

        [Tooltip("Name for the screenshot file")]
        public string FileName = "Screenshot";

        [Tooltip("Image to display the screenshot in")]
        public RawImage PreviewImage;

        CanvasGroup MenuCanvas = null;
        Texture2D Texture;

        bool _isTakingScreenshot;
        bool ScreenshotTaken;
        bool IsFeatureDisable;

        string GetPath() => ScreenshotPath + FileName + ".png";

        const string ScreenshotPath = "Assets/";

        void Awake()
        {
#if !UNITY_EDITOR
        // this feature is available only in the editor
        ScreenshotPanel.SetActive(false);
        IsFeatureDisable = true;
#else
            IsFeatureDisable = false;

            var gameMenuManager = GetComponent<InGameMenuManager>();
            DebugUtility.HandleErrorIfNullGetComponent<InGameMenuManager, TakeScreenshot>(gameMenuManager, this,
                gameObject);

            MenuCanvas = gameMenuManager.MenuRoot.GetComponent<CanvasGroup>();
            DebugUtility.HandleErrorIfNullGetComponent<CanvasGroup, TakeScreenshot>(MenuCanvas, this,
                gameMenuManager.MenuRoot.gameObject);

            LoadScreenshot();
#endif
        }

        void Update()
        {
            PreviewImage.enabled = PreviewImage.texture != null;

            if (IsFeatureDisable)
                return;

            if (_isTakingScreenshot)
            {
                MenuCanvas.alpha = 0;
                ScreenCapture.CaptureScreenshot(GetPath());
                _isTakingScreenshot = false;
                ScreenshotTaken = true;
                return;
            }

            if (ScreenshotTaken)
            {
                LoadScreenshot();
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif

                MenuCanvas.alpha = 1;
                ScreenshotTaken = false;
            }
        }

        public void OnTakeScreenshotButtonPressed()
        {
            _isTakingScreenshot = true;
        }

        void LoadScreenshot()
        {
            if (File.Exists(GetPath()))
            {
                var bytes = File.ReadAllBytes(GetPath());

                Texture = new Texture2D(2, 2);
                Texture.LoadImage(bytes);
                Texture.Apply();
                PreviewImage.texture = Texture;
            }
        }
    }
}
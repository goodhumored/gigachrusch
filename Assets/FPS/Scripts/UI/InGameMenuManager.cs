using FPS.Scripts.Game;
using FPS.Scripts.Game.Shared;
using FPS.Scripts.Gameplay.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FPS.Scripts.UI
{
    public class InGameMenuManager : MonoBehaviour
    {
        [Tooltip("Root GameObject of the menu used to toggle its activation")]
        public GameObject MenuRoot;

        [Tooltip("Master volume when menu is open")] [Range(0.001f, 1f)]
        public float VolumeWhenMenuOpen = 0.5f;

        [Tooltip("Slider component for look sensitivity")]
        public Slider LookSensitivitySlider;

        [Tooltip("Brightness slider")]
        public Slider BrightnessSlider;

        [Tooltip("Toggle component for framerate display")]
        public Toggle FramerateToggle;

        [Tooltip("GameObject for the controls")]
        public GameObject ControlImage;

        PlayerInputHandler PlayerInputsHandler;
        Health PlayerHealth;
        FramerateCounter FramerateCounter;
        // private Volume _volume;
        // private LiftGammaGain _gamma;

        void Start()
        {
            PlayerInputsHandler = FindObjectOfType<PlayerInputHandler>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerInputHandler, InGameMenuManager>(PlayerInputsHandler,
                this);

            PlayerHealth = PlayerInputsHandler.GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, InGameMenuManager>(PlayerHealth, this, gameObject);

            FramerateCounter = FindObjectOfType<FramerateCounter>();
            DebugUtility.HandleErrorIfNullFindObject<FramerateCounter, InGameMenuManager>(FramerateCounter, this);

            MenuRoot.SetActive(false);

            LookSensitivitySlider.value = PlayerInputsHandler.LookSensitivity;
            LookSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

            // if (_volume.profile.TryGet<LiftGammaGain>(out var gammaGain))
            // {
            //     _gamma = gammaGain;
            //     BrightnessSlider.value = gammaGain.gamma.value.w;
            //     BrightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            // }

            FramerateToggle.isOn = FramerateCounter.UIText.gameObject.activeSelf;
            FramerateToggle.onValueChanged.AddListener(OnFramerateCounterChanged);
        }

        void Update()
        {
            // Lock cursor when clicking outside of menu
            if (!MenuRoot.activeSelf && Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (Input.GetButtonDown(GameConstants.ButtonNamePauseMenu)
                || (MenuRoot.activeSelf && Input.GetButtonDown(GameConstants.ButtonNameCancel)))
            {
                if (ControlImage.activeSelf)
                {
                    ControlImage.SetActive(false);
                    return;
                }

                SetPauseMenuActivation(!MenuRoot.activeSelf);

            }

            if (Input.GetAxisRaw(GameConstants.AxisNameVertical) != 0)
            {
                if (EventSystem.current.currentSelectedGameObject == null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    LookSensitivitySlider.Select();
                }
            }
        }

        public void ClosePauseMenu()
        {
            SetPauseMenuActivation(false);
        }

        public void ExitGame()
        {
            SceneManager.LoadScene("IntroMenu");
        }

        void SetPauseMenuActivation(bool active)
        {
            MenuRoot.SetActive(active);

            if (MenuRoot.activeSelf)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
                AudioUtility.SetMasterVolume(VolumeWhenMenuOpen);

                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
                AudioUtility.SetMasterVolume(1);
            }

        }

        void OnMouseSensitivityChanged(float newValue)
        {
            PlayerInputsHandler.LookSensitivity = newValue;
        }

        void OnBrightnessChanged(float newValue)
        {
            // var value = _gamma.gamma.value;
            // _gamma.gamma.value.Set(value.x, value.y, value.z, newValue);
        }

        void OnFramerateCounterChanged(bool newValue)
        {
            FramerateCounter.UIText.gameObject.SetActive(newValue);
        }

        public void OnShowControlButtonClicked(bool show)
        {
            ControlImage.SetActive(show);
        }
    }
}
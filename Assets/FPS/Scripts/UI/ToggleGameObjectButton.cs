using FPS.Scripts.Game;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FPS.Scripts.UI
{
    public class ToggleGameObjectButton : MonoBehaviour
    {
        public GameObject ObjectToToggle;
        public bool ResetSelectionAfterClick;

        void Update()
        {
            if (ObjectToToggle.activeSelf && Input.GetButtonDown(GameConstants.ButtonNameCancel))
            {
                SetGameObjectActive(false);
            }
        }

        public void SetGameObjectActive(bool active)
        {
            ObjectToToggle.SetActive(active);

            if (ResetSelectionAfterClick)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
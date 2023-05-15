using FPS.Scripts.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FPS.Scripts.UI
{
    public class MenuNavigation : MonoBehaviour
    {
        public Selectable DefaultSelection;

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            EventSystem.current.SetSelectedGameObject(null);
        }

        void LateUpdate()
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (Input.GetButtonDown(GameConstants.ButtonNameSubmit)
                    || Input.GetAxisRaw(GameConstants.AxisNameHorizontal) != 0
                    || Input.GetAxisRaw(GameConstants.AxisNameVertical) != 0)
                {
                    EventSystem.current.SetSelectedGameObject(DefaultSelection.gameObject);
                }
            }
        }
    }
}
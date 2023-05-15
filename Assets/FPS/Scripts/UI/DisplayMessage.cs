using UnityEngine;

namespace FPS.Scripts.UI
{
    public class DisplayMessage : MonoBehaviour
    {
        //[Tooltip("The text that will be displayed")] [TextArea]
        //public string message;
        //
        //[Tooltip("Prefab for the message")] public GameObject messagePrefab;
        //
        //[Tooltip("Delay before displaying the message")]
        //public float delayBeforeShowing;
        //
        //float InitTime = float.NegativeInfinity;
        //bool WasDisplayed;
        //DisplayMessageManager DisplayMessageManager;
        //
        //void Start()
        //{
        //    InitTime = Time.time;
        //    DisplayMessageManager = FindObjectOfType<DisplayMessageManager>();
        //    DebugUtility.HandleErrorIfNullFindObject<DisplayMessageManager, DisplayMessage>(DisplayMessageManager,
        //        this);
        //}
        //
        // Update is called once per frame
        //void Update()
        //{
        //    if (WasDisplayed)
        //        return;
        //
        //    if (Time.time - InitTime > delayBeforeShowing)
        //    {
        //        var messageInstance = Instantiate(messagePrefab, DisplayMessageManager.DisplayMessageRect);
        //        var notification = messageInstance.GetComponent<NotificationToast>();
        //        if (notification)
        //        {
        //            notification.Initialize(message);
        //        }
        //
        //        WasDisplayed = true;
        //    }
        //}
    }
}
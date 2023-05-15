using System.Collections.Generic;
using FPS.Scripts.Game;
using FPS.Scripts.Game.Managers;
using UnityEngine;

namespace FPS.Scripts.UI
{
    public class DisplayMessageManager : MonoBehaviour
    {
        public UITable DisplayMessageRect;
        public NotificationToast MessagePrefab;

        Queue<(float timestamp, float delay, string message, NotificationToast notification)> PendingMessages = new Queue<(float timestamp, float delay, string message, NotificationToast notification)>();

        void Awake()
        {
            EventManager.AddListener<DisplayMessageEvent>(OnDisplayMessageEvent);
        }

        void OnDisplayMessageEvent(DisplayMessageEvent evt)
        {
            NotificationToast notification = Instantiate(MessagePrefab, DisplayMessageRect.transform).GetComponent<NotificationToast>();
            PendingMessages.Enqueue((Time.time, evt.DelayBeforeDisplay, evt.Message, notification));
        }

        void Update()
        {
            if (PendingMessages.TryDequeue(out var message))
            {
                if (Time.time - message.timestamp > message.delay)
                {
                    message.Item4.Initialize(message.message);
                    DisplayMessage(message.notification);
                }
            }
        }

        void DisplayMessage(NotificationToast notification)
        {
            DisplayMessageRect.UpdateTable(notification.gameObject);
            //StartCoroutine(MessagePrefab.ReturnWithDelay(notification.gameObject, notification.TotalRunTime));
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<DisplayMessageEvent>(OnDisplayMessageEvent);
        }
    }
}
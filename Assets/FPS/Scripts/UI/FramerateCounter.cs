using TMPro;
using UnityEngine;

namespace FPS.Scripts.UI
{
    public class FramerateCounter : MonoBehaviour
    {
        [Tooltip("Delay between updates of the displayed framerate value")]
        public float PollingTime = 0.5f;

        [Tooltip("The text field displaying the framerate")]
        public TextMeshProUGUI UIText;

        float AccumulatedDeltaTime = 0f;
        int AccumulatedFrameCount = 0;

        void Update()
        {
            AccumulatedDeltaTime += Time.deltaTime;
            AccumulatedFrameCount++;

            if (AccumulatedDeltaTime >= PollingTime)
            {
                int framerate = Mathf.RoundToInt((float) AccumulatedFrameCount / AccumulatedDeltaTime);
                UIText.text = framerate.ToString();

                AccumulatedDeltaTime = 0f;
                AccumulatedFrameCount = 0;
            }
        }
    }
}
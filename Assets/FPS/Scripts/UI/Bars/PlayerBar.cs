using System;
using FPS.Scripts.Game;
using FPS.Scripts.Game.Shared;
using FPS.Scripts.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FPS.Scripts.UI
{
    public class PlayerBar : MonoBehaviour
    {
        public Image barFillImage;
        public TextMeshProUGUI textMesh;
        private float _currentValue;
        private float _maxValue;

        public void SetMaxValue(float newMaxValue)
        {
            _maxValue = newMaxValue;
            ReDraw();
        }

        public void SetCurrentValue(float newValue)
        {
            if (newValue > _maxValue) _currentValue = _maxValue;
            _currentValue = newValue;
            ReDraw();
        }

        private void ReDraw()
        {
            barFillImage.fillAmount = _currentValue / _maxValue;
            textMesh.text = Mathf.Ceil(_currentValue) + "/" + _maxValue;
        }
    }
}
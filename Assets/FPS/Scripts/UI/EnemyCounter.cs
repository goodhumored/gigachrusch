using FPS.Scripts.AI;
using FPS.Scripts.Game;
using UnityEngine;
using UnityEngine.UI;

namespace FPS.Scripts.UI
{
    public class EnemyCounter : MonoBehaviour
    {
        [Header("Enemies")] [Tooltip("Text component for displaying enemy objective progress")]
        public Text EnemiesText;

        EnemyManager m_EnemyManager;

        void Awake()
        {
            m_EnemyManager = FindObjectOfType<EnemyManager>();
            DebugUtility.HandleErrorIfNullFindObject<EnemyManager, EnemyCounter>(m_EnemyManager, this);
        }

        void Update()
        {
            EnemiesText.text = m_EnemyManager.NumberOfEnemiesRemaining + "/" + m_EnemyManager.NumberOfEnemiesTotal;
        }
    }
}
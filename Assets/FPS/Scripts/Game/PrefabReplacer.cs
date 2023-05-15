using System.Collections.Generic;
using UnityEngine;

namespace FPS.Scripts.Game
{
    public class PrefabReplacer : MonoBehaviour
    {
        [System.Serializable]
        public struct ReplacementDefinition
        {
            public GameObject SourcePrefab;
            public GameObject TargetPrefab;
        }

        public bool SwitchOrder;
        public List<ReplacementDefinition> Replacements = new List<ReplacementDefinition>();
    }
}
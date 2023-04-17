using System;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

namespace Unity.FPS.Game
{
    public class Wall : MonoBehaviour
    {
        public static GameObject door;
        public static GameObject window;
        [Range(0, 1f)]
        public float chanceToSpawn = .15f;
    }
}
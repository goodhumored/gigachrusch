using FPS.Scripts.Game;
using UnityEngine;

namespace FPS.Scripts.UI
{
    public class CompassElement : MonoBehaviour
    {
        [Tooltip("The marker on the compass for this element")]
        public CompassMarker CompassMarkerPrefab;

        [Tooltip("Text override for the marker, if it's a direction")]
        public string TextDirection;

        Compass Compass;

        void Awake()
        {
            Compass = FindObjectOfType<Compass>();
            DebugUtility.HandleErrorIfNullFindObject<Compass, CompassElement>(Compass, this);

            var markerInstance = Instantiate(CompassMarkerPrefab);

            markerInstance.Initialize(this, TextDirection);
            Compass.RegisterCompassElement(transform, markerInstance);
        }

        void OnDestroy()
        {
            Compass.UnregisterCompassElement(transform);
        }
    }
}
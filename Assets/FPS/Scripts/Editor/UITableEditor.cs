using FPS.Scripts.UI;
using UnityEditor;
using UnityEngine;

namespace FPS.Scripts.Editor
{
    // The Editor for the UITable component to add an Update button

    [CustomEditor(typeof(UITable), true)]
    public class UITableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            UITable myTarget = (UITable)target;
            DrawDefaultInspector();

            if (GUILayout.Button("Update"))
            {
                myTarget.UpdateTable(null);
            }
        }
    }
}

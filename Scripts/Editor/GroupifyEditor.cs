using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Groupify
{
    [CustomEditor(typeof(Groupify))]
    public class GroupifyEditor : Editor
    {
        private Groupify groupify { get { return (Groupify)target; } }
        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Groups: " + groupify.GroupsCount);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Open groupify window", EditorStyles.miniButton))
                GroupifyWindow.Init(groupify);
            if (GUILayout.Button("About", EditorStyles.miniButton)) { }
            GUILayout.EndVertical();
        }
    }
}

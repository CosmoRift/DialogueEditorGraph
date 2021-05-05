using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace DialogueGraphEditor
{
    [CustomEditor(typeof(DialogueSO))]
    public class DialogueSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DialogueSO dialogueSO = (DialogueSO)target;
            if (GUILayout.Button("Open Editor"))
            {
                DialogueGraph.CreateGraphViewWindow(dialogueSO);

            }
            base.OnInspectorGUI();

        }

    }
}

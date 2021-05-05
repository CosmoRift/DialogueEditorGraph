using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UltEvents;
using System;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
namespace DialogueGraphEditor
{
    [System.Serializable]
    public class EventWindow : EditorWindow
    {
        public UltEvent currentEvent;
        public Condition currentCondition;
        public DialogueFunctionNode dfn;
        static bool showingEvent;
        static VisualElement visualElement;
       
        public static void OpenWindow(ref Condition condition, VisualElement ve = null, UnityEngine.Object o = null, Rect position = default(Rect))
        {
            EventWindow window = (EventWindow)GetWindow(typeof(EventWindow));
            window.currentCondition = condition;
            if (window.currentCondition.target == null)
            {
                if (o != null)
                {
                    window.currentCondition.target = o;
                }
            }
            showingEvent = false;
            visualElement = ve;
            ShowWindow(window, position);
        }
        public static void OpenWindow(ref UltEvent ultevent, VisualElement ve = null, UnityEngine.Object o = null, Rect position = default(Rect))
        {
            EventWindow window = (EventWindow)GetWindow(typeof(EventWindow));
            if (o != null)
            {
                ultevent.useAlternativeDefaultTarget = true;
                ultevent.alternativeDefaultTarget = o;
            }
            window.currentEvent = ultevent;
            showingEvent = true;
            visualElement = ve;

            ShowWindow(window, position);
        }
        public static void ShowWindow(EventWindow window, Rect position = default(Rect))
        {


            window.Show();
            window.minSize = new Vector2(200, 100);
            if (position != default(Rect))
            {
                window.position = position;
            }
        }
        void OnGUI()
        {

            SerializedObject so = new SerializedObject(this);
            SerializedProperty sp = so.FindProperty("currentEvent");

            SerializedProperty sp2 = so.FindProperty("currentCondition");
            EditorGUI.BeginChangeCheck();

            if (showingEvent)
            {
                EditorGUILayout.PropertyField(sp);
            }
            else
            {
                EditorGUILayout.PropertyField(sp2);
            }
            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
                visualElement?.MarkDirtyRepaint();

            }
            so.ApplyModifiedProperties();
        }
        private void OnDestroy()
        {

            visualElement?.MarkDirtyRepaint();
        }
    }
}


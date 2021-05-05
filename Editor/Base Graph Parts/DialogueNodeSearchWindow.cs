using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace DialogueGraphEditor
{

    public class DialogueNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private EditorWindow window;
        private DialogueGraphView graphView;

        private Texture2D indentationIcon;

        public void Configure(EditorWindow window, DialogueGraphView graphView)
        {
            this.window = window;
            this.graphView = graphView;

            //Transparent 1px indentation icon as a hack
            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
                new SearchTreeGroupEntry(new GUIContent("Nodes"), 1),
                new SearchTreeEntry(new GUIContent("Text Node", indentationIcon))
                {
                    level = 2, userData = new DialogueTextNode()
                },
                 new SearchTreeEntry(new GUIContent("Function Node", indentationIcon))
                {
                    level = 2, userData = new DialogueFunctionNode()
                },
                  new SearchTreeEntry(new GUIContent("Branch Node", indentationIcon))
                {
                    level = 2, userData = new DialogueBranchNode()
                },
                    new SearchTreeEntry(new GUIContent("Exit Node", indentationIcon))
                {
                    level = 2, userData = new DialogueExitNode()
                },
                new SearchTreeEntry(new GUIContent("Comment Block",indentationIcon))
                {
                    level = 1,
                    userData = new Group()
                }
            };

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 mousePosition = window.rootVisualElement.ChangeCoordinatesTo(window.rootVisualElement.parent,
                context.screenMousePosition - window.position.position);
            Vector2 graphMousePosition = graphView.contentViewContainer.WorldToLocal(mousePosition);
            if (SearchTreeEntry.userData is Group)
            {
                Rect rect = new Rect(graphMousePosition, DialogueGraphView.defaultCommentBlockSize);
                graphView.CreateCommentBlock(rect);
                return true;
            }
            else if (SearchTreeEntry.userData is DialogueNode)
            {
                graphView.CreateNewDialogueNode(graphMousePosition, ((DialogueNode)SearchTreeEntry.userData).type);

                return true;




            }
            return false;
        }
    }
}


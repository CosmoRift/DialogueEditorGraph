using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
namespace DialogueGraphEditor
{
    [Serializable]
    public class DialogueGraph : EditorWindow
    {
        [SerializeField]
        public static DialogueSO dialogue;
        DialogueGraphView graphView;
        static DialogueGraph windowInstance;

        static bool autoSave;

        public static int counter = 0;
        /// <summary>
        /// Cap that limits how many times can response travel through nodes ()
        /// </summary>
        public static int responseJumpCap = 30;
        public static void CreateGraphViewWindow(DialogueSO dialogueSO)
        {
            dialogue = dialogueSO;

            windowInstance = GetWindow<DialogueGraph>();
            windowInstance.minSize = new Vector2(100, 100);
            windowInstance.titleContent = new GUIContent("Graph");
            windowInstance.wantsMouseMove = true;


        }
        public static void SetChanged()
        {
            if (!autoSave)
            {
                windowInstance.hasUnsavedChanges = true;
            }
        }

        public override void SaveChanges()
        {
            base.SaveChanges();
            Save(dialogue);
        }
        private void ConstructGraphView()
        {
            graphView = new DialogueGraphView(this)
            {
                name = "Graph",

            };
            graphView.StretchToParentSize();

            rootVisualElement.Add(graphView);

            //   graphView.UpdateViewTransform(new Vector3(graphView.layout.width / 2, graphView.layout.height / 2, 0), Vector3.one);

        }
        protected void GenerateToolbar()
        {
            Toolbar toolbar = new Toolbar();




            if (dialogue != null)
            {
                SerializedObject diz = new SerializedObject(dialogue);
                ObjectField of = new ObjectField();
                of.objectType = typeof(DialogueSO);
                of.Bind(diz);
                of.value = dialogue;
                of.RegisterValueChangedCallback(x =>
                {
                    if (autoSave)
                    {
                        Save((DialogueSO)x.previousValue);
                    }
                    Load((DialogueSO)x.newValue);
                });

                toolbar.Add(of);

                toolbar.Add(new Label("  "));

                toolbar.Add(new Button(() => Save(dialogue)) { text = "Save Data" });

                toolbar.Add(new Label("  "));

                toolbar.Add(new Button(() => Load(dialogue)) { text = "Load Data" });

                toolbar.Add(new Label("  "));


                Toggle autoSaveToggle = new Toggle("Auto save");
                autoSaveToggle.name = "autoSaveToggle";
                autoSaveToggle.Q<Label>().style.minWidth = 0;

                autoSaveToggle.value = autoSave;
                autoSaveToggle.RegisterValueChangedCallback(x =>
                {
                    autoSave = x.newValue;

                    EditorPrefs.SetBool("autoSave", autoSave);

                    if (!autoSave)
                    {
                        Save(dialogue);
                    }
                });
                toolbar.Add(autoSaveToggle);
            }

            /*  toolbar.Add(new Button(
                   () =>
                   {
                       Debug.Log(rootVisualElement.layout.width + "|+" + rootVisualElement.layout.height);
                   })
              { text = "w+h" }
                );*/
            /*  toolbar.Add(new Button(
                  () =>
                  {
                      Type t = typeof(GraphView);
                      Debug.Log(t);
                      PropertyInfo p = t.GetProperty("clipboard", BindingFlags.Instance | BindingFlags.NonPublic);
                      Debug.Log(p);
                      object o = p.GetValue(graphView);
                      Debug.Log((string)o);

                  })
              { text = "display clipboard" }
              );*/

            toolbar.Add(new Button(
                   () =>
                   {
                       EditorGUIUtility.systemCopyBuffer = graphView.SerializeGraphElementsCustom(graphView.graphElements.ToList());

                   })
            { text = "Make Backup to clipboard" }
                );
            toolbar.Add(new Button(
                 () =>
                 {
                     int a = EditorUtility.DisplayDialogComplex("Choose", "Do you want to remove all items before loading or just load (can duplicate)", "Remove and load", "Cancel", "Load without removing");
                     if (a == 2)
                     {
                         graphView.UnserializeAndPasteCustom("", EditorGUIUtility.systemCopyBuffer);

                     }
                     else if (a == 0)
                     {
                         RemoveAll();

                         graphView.UnserializeAndPasteCustom("", EditorGUIUtility.systemCopyBuffer);

                     }
                 })
            { text = "Load Backup from clipboard" }
              );

            rootVisualElement.Add(toolbar);
        }

        /// <summary>
        /// Save all data to DialogueSO
        /// </summary>
        /// <param name="dialogueSO">Choosen DialogueSO.</param>
        public void Save(DialogueSO dialogueSO)
        {
            if (graphView.GetNodes().Count < 1)
            {
                Debug.LogError("There was a try to save empty graph!");
                return;
            }
            dialogueSO.nodes.Clear();
            dialogueSO.links.Clear();
            List<DialogueNode> nodes = graphView.GetNodes();
            foreach (DialogueNode dn in nodes)
            {
                dialogueSO.nodes.Add(dn.Save());
                dialogueSO.links.AddRange(dn.GetConnections());
            }

            dialogueSO.comments.Clear();

            List<Group> CommentBlocks = graphView.graphElements.ToList().Where(x => x is Group).Cast<Group>().ToList();
            foreach (Group block in CommentBlocks)
            {
                List<string> nodesInside = block.containedElements.Where(x => x is DialogueNode).Cast<DialogueNode>().Select(x => x.guid)
                    .ToList();

                dialogueSO.comments.Add(new CommentBlockData
                {
                    ChildNodes = nodesInside,
                    Title = block.title,
                    Position = block.GetPosition().position
                });
            }

        }
        /// <summary>
        /// Loads graph from DialogueSO
        /// </summary>
        /// <param name="dialogueSO">Choosen DialogueSO.</param>
        public void Load(DialogueSO dialogueSO)
        {
            RemoveAll();
            List<DialogueNodeData> nodes = dialogueSO.nodes;
            foreach (DialogueNodeData dnd in nodes)
            {
                DialogueNode dn = graphView.CreateNode(dnd);
                graphView.AddElement(dn);
            }


            List<DialogueNode> dnodes = graphView.GetNodes();
            foreach (CommentBlockData commentBlockData in dialogueSO.comments)
            {
                Group block = graphView.CreateCommentBlock(new Rect(commentBlockData.Position, DialogueGraphView.defaultCommentBlockSize),
                     commentBlockData);
                block.AddElements(dnodes.Where(x => commentBlockData.ChildNodes.Contains(x.guid)));
            }
            List<NodeLinkData> links = dialogueSO.links;
            foreach (NodeLinkData nld in links)
            {
                DialogueNode nodeIn = dnodes.Find(x => nld.inputNodeGUID == x.guid);
                DialogueNode nodeOut = dnodes.Find(x => nld.outputNodeGUID == x.guid);

                Port outPort = nodeOut.GetPort(Direction.Output, nld.outputPortGUID);
                Port inPort = nodeIn.inputPort;

                Edge tempEdge = new Edge()
                {
                    output = outPort,
                    input = inPort
                };
                inPort.Connect(tempEdge);
                outPort.Connect(tempEdge);
                graphView.Add(tempEdge);
            }

        }
        void RemoveAll()
        {
            graphView.ClearNodes();
            foreach (Edge e in graphView.edges.ToList())
            {
                graphView.RemoveElement(e);
            }
            List<Group> CommentBlocks = graphView.graphElements.ToList().Where(x => x is Group).Cast<Group>().ToList();
            foreach (Group commentBlock in CommentBlocks)
            {
                graphView.RemoveElement(commentBlock);
            }
        }
        private void OnEnable()
        {
            //Loading data from editorPrefs
            autoSave = EditorPrefs.GetBool("autoSave");
            if (dialogue == null)
            {
                dialogue = AssetDatabase.LoadAssetAtPath<DialogueSO>(EditorPrefs.GetString("SOpath"));
            }

            ConstructGraphView();
            GenerateToolbar();
            Load(dialogue);


        }
        private void OnDisable()
        {
            //Saving data to editorPrefs
            EditorPrefs.SetBool("autoSave", autoSave);
            EditorPrefs.SetString("SOpath", AssetDatabase.GetAssetPath(dialogue));

            //autosaving
            if (autoSave)
            {
                Save(dialogue);
            }
            rootVisualElement.Remove(graphView);
        }
        private void OnDestroy()
        {
        }

    }

}
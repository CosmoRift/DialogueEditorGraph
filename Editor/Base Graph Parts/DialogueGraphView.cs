using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
namespace DialogueGraphEditor
{

    public class DialogueGraphView : GraphView
    {

        public static readonly Vector2 defaultNodeSize = new Vector2(200, 150);
        public static readonly Vector2 defaultCommentBlockSize = new Vector2(300, 200);
        private DialogueGraph window;
        private DialogueNodeSearchWindow searchWindow;

        void RegisterEvents()
        {

            serializeGraphElements = SerializeGraphElementsCustom;
            canPasteSerializedData = CanPasteSerializedDataCustom;
            unserializeAndPaste = UnserializeAndPasteCustom;

        }
        public DialogueGraphView(DialogueGraph window)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraphStyleSheet"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);


            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            GridBackground grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
            this.window = window;
            window.wantsMouseMove = true;

            AddSearchWindow(window);
            graphViewChanged += Change;
            RegisterEvents();



        }

        GraphViewChange Change(GraphViewChange change)
        {
            DialogueGraph.SetChanged();
            return change;
        }
        private void AddSearchWindow(DialogueGraph editorWindow)
        {
            searchWindow = ScriptableObject.CreateInstance<DialogueNodeSearchWindow>();
            searchWindow.Configure(editorWindow, this);
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }


        public void CreateNewDialogueNode(Vector2 position, DialogueNodeType type)
        {
            DialogueNodeData dnd = new DialogueNodeData();
            dnd.guid = Guid.NewGuid().ToString();
            dnd.position = position;
            dnd.type = type;
            AddElement(CreateNode(dnd));

        }
        /// <summary>
        /// Clears all nodes.
        /// </summary>
        public void ClearNodes()
        {
            List<DialogueNode> nodes = GetNodes();
            foreach (DialogueNode node in nodes)
            {
                RemoveElement(node);
            }
        }
        /// <summary>
        /// Gets all nodes.
        /// </summary>
        /// <returns>list of all nodes.</returns>
        public List<DialogueNode> GetNodes()
        {
            return nodes.ToList().Cast<DialogueNode>().ToList();
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach((port) =>
            {
                if (!startPort.connected)
                {
                    if (startPort != port && startPort.node != port.node && startPort.direction != port.direction) //check if iscnt already connected
                {
                        if (startPort.connections.ToList().TrueForAll(x => x.input != port && x.output != port))
                        {
                            compatiblePorts.Add(port);
                        }
                    }
                }
            });

            return compatiblePorts;
        }
        /// <summary>
        /// Makes node from data. Remember to add it to graph.
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Created node.</returns>
        public DialogueNode CreateNode(DialogueNodeData data)
        {
            switch (data.type)
            {
                case DialogueNodeType.Text:
                    {
                        DialogueTextNode tempDialogueNode = new DialogueTextNode();
                        tempDialogueNode.SetupFromData(data);
                        return tempDialogueNode;
                    }
                case DialogueNodeType.Function:
                    {
                        DialogueFunctionNode tempDialogueNode = new DialogueFunctionNode();
                        tempDialogueNode.SetupFromData(data);
                        return tempDialogueNode;
                    }
                case DialogueNodeType.Branch:
                    {
                        DialogueBranchNode tempDialogueNode = new DialogueBranchNode();
                        tempDialogueNode.SetupFromData(data);
                        return tempDialogueNode;
                    }
                case DialogueNodeType.Start:
                    {
                        DialogueStartNode tempDialogueNode = new DialogueStartNode();
                        tempDialogueNode.SetupFromData(data);
                        return tempDialogueNode;
                    }
                case DialogueNodeType.Exit:
                    {
                        DialogueExitNode tempDialogueNode = new DialogueExitNode();
                        tempDialogueNode.SetupFromData(data);
                        return tempDialogueNode;
                    }
            }
            return null;
        }
        public Group CreateCommentBlock(Rect rect, CommentBlockData commentBlockData = null)
        {
            if (commentBlockData == null)
                commentBlockData = new CommentBlockData();
            Group group = new Group
            {
                autoUpdateGeometry = true,
                title = commentBlockData.Title
            };
            AddElement(group);
            group.SetPosition(rect);
            return group;
        }

        public string SerializeGraphElementsCustom(IEnumerable<GraphElement> elements)
        {
            //Used for packing all objects fro json
            SerielizedGraphElementsPack sgep = new SerielizedGraphElementsPack();

            foreach (DialogueNode dn in elements.Where(x => x is DialogueNode).Cast<DialogueNode>())
            {
                DialogueNodeData dnd = dn.Save();
                sgep.dnds.Add(dnd);
            }
            foreach (Group block in elements.Where(x => x is Group).Cast<Group>())
            {
                List<string> nodesInside = block.containedElements.Where(x => x is DialogueNode).Cast<DialogueNode>().Select(x => x.guid).ToList();


                sgep.cbds.Add(new CommentBlockData
                {
                    ChildNodes = nodesInside,
                    Title = block.title,
                    Position = block.GetPosition().position
                });


            }
            return JsonUtility.ToJson(sgep);
        }
        bool CanPasteSerializedDataCustom(string data)
        {
            //TODO: implement better check. It is check in the UnserializeAndPasteCustom tho.
            return data.Contains("{") && data.Contains("}");
        }


        public void UnserializeAndPasteCustom(string operationName, string data)
        {
            try
            {
                //You need to change guids to avoid pasting same data
                Dictionary<string, string> guidSwap = new Dictionary<string, string>();
                SerielizedGraphElementsPack sgep = JsonUtility.FromJson<SerielizedGraphElementsPack>(data);

                foreach (DialogueNodeData dnd in sgep.dnds)
                {
                    dnd.position += new Vector2(10, 10);

                    string newGuid = Guid.NewGuid().ToString();
                    guidSwap.Add(dnd.guid, newGuid);
                    dnd.guid = newGuid;
                    AddElement(CreateNode(dnd));
                }

                List<DialogueNode> dnodes = GetNodes();
                foreach (CommentBlockData commentBlockData in sgep.cbds)
                {

                    List<string> nodesInside = commentBlockData.ChildNodes;

                    for (int i = 0; i < nodesInside.Count; i++)
                    {
                        string newGuid = nodesInside[i];
                        if (guidSwap.TryGetValue(nodesInside[i], out newGuid))
                        {
                            nodesInside[i] = newGuid;
                        }
                    }
                    Group block = CreateCommentBlock(new Rect(commentBlockData.Position + new Vector2(10, 10), defaultCommentBlockSize),
                         commentBlockData);

                    block.AddElements(dnodes.Where(x => commentBlockData.ChildNodes.Contains(x.guid)));
                }

            }
            catch
            {

                EditorUtility.DisplayDialog("Error", "There was an error trying to copy nodes and/or comment blocks!", "Ok");

            }
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.AppendSeparator();
            if (evt.target is GraphView || evt.target is Node || evt.target is Group)
            {
                evt.menu.AppendAction("Collapse All Nodes", (a) => { CollapseAllNodes(); },
                    (a) => { return true ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled; });
            }
            if (evt.target is GraphView || evt.target is Node || evt.target is Group)
            {
                evt.menu.AppendAction("Expand All Nodes", (a) => { ExpandAllNodes(); },
                    (a) => { return true ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled; });
            }
            evt.menu.AppendSeparator();
            if (evt.target is GraphView || evt.target is Node || evt.target is Group)
            {
                evt.menu.AppendAction("Update All Nodes", (a) => { UpdateAllNodes(); },
                    (a) => { return true ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled; });
            }
        }
        void CollapseAllNodes()
        {
            List<DialogueNode> nodes = GetNodes();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Collapse(false);
            }
        }
        void ExpandAllNodes()
        {
            List<DialogueNode> nodes = GetNodes();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Collapse(true);
            }
        }
        void UpdateAllNodes()
        {
            List<DialogueNode> nodes = GetNodes();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Refresh();
            }
        }
        //Used to pack all data for json
        [Serializable]
        class SerielizedGraphElementsPack
        {
            public List<DialogueNodeData> dnds = new List<DialogueNodeData>();
            public List<CommentBlockData> cbds = new List<CommentBlockData>();

        }
    }
}
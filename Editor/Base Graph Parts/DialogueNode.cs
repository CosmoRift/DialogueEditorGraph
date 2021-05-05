using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueGraphEditor
{

    [Serializable]
    public class DialogueNode : Node
    {

        public string guid;
        public virtual DialogueNodeType type => DialogueNodeType.Null;
        [SerializeReference]
        public DialogueNodeData data;
        public Port inputPort;
        /// <summary>
        /// List of all elements that want to be collaped but are in diffrent VEs.
        /// </summary>
        public List<VisualElement> collapsableVisualElements = new List<VisualElement>();
        /// <summary>
        /// Setup node from data.
        /// </summary>
        /// <param name="data">Node data.</param>
        public virtual void SetupFromData(DialogueNodeData data)
        {
            //Loading data and style
            styleSheets.Add(Resources.Load<StyleSheet>("NodeStyleSheet"));
            this.data = data;
            this.guid = data.guid;

            //Input port
            inputPort = AddPort(Direction.Input, "input");


            SetPosition(new Rect(data.position, DialogueGraphView.defaultNodeSize));
            Refresh();
        }
        protected override void ToggleCollapse()
        {
            foreach (VisualElement ve in collapsableVisualElements)
            {
                if (ve != null)
                {
                    ve.style.display = (!expanded) ? DisplayStyle.Flex : DisplayStyle.None;
                    ve.visible = (!expanded);
                    ve.MarkDirtyRepaint();
                }
            }
            base.ToggleCollapse();

        }
        public void Collapse(bool value)
        {
            expanded = !value;
            ToggleCollapse();
        }
        /// <summary>
        /// Refresh function that should refresh whole node.
        /// </summary>
        public void Refresh()
        {
            expanded = false;
            this.MarkDirtyRepaint();
            ToggleCollapse();
            RefreshPorts();
            RefreshExpandedState();
        }
        /// <summary>
        /// Function that changes response text, it is used to move response from text and end nodes to previous text node through other nodes. Counts steps to avoid looping.
        /// </summary>
        /// <param name="port">Port that is connected to response display.</param>
        /// <param name="responseText">Response display text.</param>
        public virtual bool ChangeResponseDisplay(Port port, string responseText)
        {
            DialogueGraph.counter++;
            return DialogueGraph.counter > DialogueGraph.responseJumpCap;
        }
        /// <summary>
        /// Function used to create ports.
        /// </summary>
        /// <param name="direction">Port direction.</param>
        /// <param name="portData">Port data (mainly guid).</param>
        /// <returns>If it looped over 20 times.</returns>
        public Port AddPort(Direction direction, string portData)
        {
            Port port = InstantiatePort(Orientation.Horizontal, direction, Port.Capacity.Multi, typeof(string));
            port.portColor = Color.white;
            if (direction == Direction.Input)
            {
                inputContainer.Add(port);
                port.name = "input";
            }
            else
            {
                outputContainer.Add(port);
                port.name = "output";
            }
            Label portLabel = port.contentContainer.Q<Label>("type");
            portLabel.text = "";
            port.userData = portData;
            return port;
        }
        /// <summary>
        /// Compares port to port base info.
        /// </summary>
        /// <param name="port">Port.</param>
        /// <param name="direction">Direction of port.</param>
        /// <param name="portData">Port data (mainly guid).</param>
        /// <returns>If data is matching.</returns>
        public bool ComparePortData(Port port, Direction direction, string portData)
        {
            if (port.direction == direction)
            {
                if (port.userData is string)
                {
                    if (((string)port.userData) == portData)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Compares ports.
        /// </summary>
        /// <param name="port1">Port 1.</param>
        /// <param name="port2">Port 2.</param>
        /// <returns>If data is matching.</returns>
        public bool ComparePortData(Port port1, Port port2)
        {
            return ComparePortData(port1, port2.direction, (string)port2.userData);
        }
        /// <summary>
        /// Gets port by data. Overrriden by node types.
        /// </summary>
        /// <param name="direction">Direction of node.</param>
        /// <param name="portData">Port data (mainly guid).</param>
        /// <returns>If data is matching.</returns>
        public virtual Port GetPort(Direction direction, string portData)
        {
            if (ComparePortData(inputPort, direction, portData))
            {
                return inputPort;
            }
            return null;
        }
        /* public List<DialogueNode> ConnectedNodes(Port port)
         {
             List<DialogueNode> dns = new List<DialogueNode>();
             if (port.direction == Direction.Input)
             {
                 foreach (Edge e in port.connections)
                 {
                     dns.Add((DialogueNode)e.output.node);
                 }
             }
             else
             {
                 foreach (Edge e in port.connections)
                 {
                     dns.Add((DialogueNode)e.input.node);
                 }
             }
             return dns;
         }*/
        /// <summary>
        /// Function that returns all connection to input port of this node.
        /// </summary>
        /// <returns>Input port connection.</returns>
        public virtual List<NodeLinkData> GetConnections()
        {
            List<NodeLinkData> nld = new List<NodeLinkData>();

            foreach (Edge e in inputPort.connections)
            {
                nld.Add(new NodeLinkData()
                {
                    inputNodeGUID = guid,
                    outputNodeGUID = ((DialogueNode)e.output.node).guid,
                    outputPortGUID = (string)e.output.userData,

                });

            }
            return nld;
        }
        /// <summary>
        /// Saving function. Overriden by node types to save their respecting fields.
        /// </summary>
        /// <returns>Saved node data.</returns>
        public virtual DialogueNodeData Save()
        {
            return new DialogueNodeData()
            {
                guid = this.guid,
                position = GetPosition().position,
                type = type,
            };
        }


    }

}
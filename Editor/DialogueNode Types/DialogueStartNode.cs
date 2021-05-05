
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using UltEvents;
namespace DialogueGraphEditor
{

    [Serializable]
    public class DialogueStartNode : DialogueNode
    {
        public Port outputPort;
        public override DialogueNodeType type => DialogueNodeType.Start;
        public override void SetupFromData(DialogueNodeData data)
        {

            title = "Start";


            base.SetupFromData(data);
            //  var port = inputContainer.Q<Port>("input");

            capabilities &= ~Capabilities.Deletable;
            inputPort.style.display = DisplayStyle.None;
            inputContainer.style.display = DisplayStyle.None;

            outputPort = AddPort(Direction.Output, "output");
            Refresh();
        }
        public override bool ChangeResponseDisplay(Port p, string text)
        {


            bool b = base.ChangeResponseDisplay(p, text);
            if (b)
            {
                Debug.LogError("You made a loop!");

                return true;
            }

            return b;
        }
        public override Port GetPort(Direction direction, string portData)
        {
            Port p = base.GetPort(direction, portData);
            if (p != null)
            {
                return p;
            }
            if (ComparePortData(outputPort, direction, portData))
            {
                return outputPort;
            }
            return null;
        }

        public override DialogueNodeData Save()
        {
            DialogueNodeData dnd = base.Save();
            dnd.outputGuids.Add((string)outputPort.userData);

            return dnd;
        }
    }
}
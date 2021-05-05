using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UltEvents;
namespace DialogueGraphEditor
{
    [Serializable]
    public class DialogueFunctionNode : DialogueNode
    {

        public UltEvent OnExecute = new UltEvent();

        public Port outputPort;
        public override DialogueNodeType type => DialogueNodeType.Function;
        public override void SetupFromData(DialogueNodeData data)
        {

            title = "Function Node";
            OnExecute = data.OnExecute;


            base.SetupFromData(data);


            Button openEventDataButton = new Button(() => { EventWindow.OpenWindow(ref OnExecute, this, DialogueGraph.dialogue); })
            {
                text = "Open Function Data"
            };


            /* SerializedProperty sp = so.FindProperty("OnExecute");
             PropertyField pf = new PropertyField(sp);*/
            extensionContainer.Add(openEventDataButton);
            if (OnExecute.PersistentCallsList != null)
            {
                List<PersistentCall> pcs = OnExecute.PersistentCallsList;

                for (int i = 0; i < pcs.Count; i++)
                {
                    string text = pcs[i].MethodName + "(";
                    /*  for (int j = 0; j < pcs[i].PersistentArguments.Length; j++)
                      {
                          text += pcs[i].PersistentArguments[j].Value .ToString() + ",";
                      }*/
                    Label label = new Label(text + ")");
                    extensionContainer.Add(label);

                }
            }
            outputPort = AddPort(Direction.Output, "output");
            Refresh();
        }
        public override bool ChangeResponseDisplay(Port p, string text)
        {

            p.contentContainer.Q<Label>("type").text = text;
            inputPort.contentContainer.Q<Label>("type").text = text;

            bool b = base.ChangeResponseDisplay(p, text);
            if (b)
            {
                Debug.LogError("You made a loop!");

                return true;
            }
            foreach (Edge e in inputPort.connections)
            {
                ((DialogueNode)e.input.node).ChangeResponseDisplay(e.output, text);
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
            dnd.OnExecute = OnExecute;
            return dnd;
        }
    }
}
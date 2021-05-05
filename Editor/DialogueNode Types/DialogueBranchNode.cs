using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace DialogueGraphEditor
{
    [Serializable]
    public class DialogueBranchNode : DialogueNode
    {

        public Condition OnCheck = new Condition();

        public Port outputPortTrue;
        public Port outputPortFalse;
        string buffer;
        public override DialogueNodeType type => DialogueNodeType.Branch;
        public override void SetupFromData(DialogueNodeData data)
        {

            title = "Branch Node";
            OnCheck = data.OnCheck;


            base.SetupFromData(data);
            Button openEventDataButton = new Button(() => { EventWindow.OpenWindow(ref OnCheck, this, DialogueGraph.dialogue); })
            {
                text = "Open Branch Data"
            };


            /* SerializedProperty sp = so.FindProperty("OnExecute");
             PropertyField pf = new PropertyField(sp);*/
            extensionContainer.Add(openEventDataButton);
            if (OnCheck.func != null)
            {


                string text = OnCheck.methodName + "(";
                /*  for (int j = 0; j < pcs[i].PersistentArguments.Length; j++)
                  {
                      text += pcs[i].PersistentArguments[j].Value .ToString() + ",";
                  }*/
                Label label = new Label(text + ")");
                extensionContainer.Add(label);
            }
            outputPortTrue = AddPort(Direction.Output, "outputTrue");
            outputPortTrue.portColor = Color.green;

            outputPortFalse = AddPort(Direction.Output, "outputFalse");
            outputPortFalse.portColor = Color.red;

            Refresh();
        }
        public override bool ChangeResponseDisplay(Port p, string text)
        {
            p.contentContainer.Q<Label>("type").text = text;

            buffer = "[" + outputPortTrue.contentContainer.Q<Label>("type").text + " / " + outputPortFalse.contentContainer.Q<Label>("type").text + "]";
            inputPort.contentContainer.Q<Label>("type").text = buffer;
            bool b = base.ChangeResponseDisplay(p, text);
            if (b)
            {
                Debug.LogError("You made a loop!");
                return true;
            }
            foreach (Edge e in inputPort.connections)
            {
                ((DialogueNode)e.output.node).ChangeResponseDisplay(e.output, buffer);
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
            if (ComparePortData(outputPortTrue, direction, portData))
            {
                return outputPortTrue;
            }
            if (ComparePortData(outputPortFalse, direction, portData))
            {
                return outputPortFalse;
            }
            return null;
        }

        public override DialogueNodeData Save()
        {
            DialogueNodeData dnd = base.Save();
            dnd.OnCheck = OnCheck;
            return dnd;
        }
    }
}

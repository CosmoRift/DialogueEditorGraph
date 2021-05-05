using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueGraphEditor
{
    public class DialogueExitNode : DialogueNode
    {
        public string inputText;


        public override DialogueNodeType type => DialogueNodeType.Exit;
        public override void SetupFromData(DialogueNodeData data)
        {
            title = "Exit";




            base.SetupFromData(data);
            inputText = data.inputText;

            #region responseTextField
            TextField responseTextField = new TextFieldWithPlaceholder("Response...", "Response to previous text");
            responseTextField.name = "inputTextField";

            responseTextField.SetValueWithoutNotify(inputText);
            responseTextField.RegisterValueChangedCallback(evt =>
            {
                inputText = evt.newValue;
                foreach (Edge e in inputPort.connections)
                {
                    DialogueGraph.counter = 0;
                    ((DialogueNode)e.output.node).ChangeResponseDisplay(e.output, inputText);
                }
            });
            responseTextField.multiline = true;
            #endregion

            inputContainer.Add(responseTextField);










            //   previewImg.BringToFront();
            Refresh();
        }

        public override bool ChangeResponseDisplay(Port p, string text)
        {
            // p.contentContainer.Q<Label>("type").text = text;
            return base.ChangeResponseDisplay(p, text);
        }

        public override Port GetPort(Direction direction, string portData)
        {

            Port p = base.GetPort(direction, portData);
            if (p != null)
            {
                return p;
            }


            return null;
        }
        public override DialogueNodeData Save()
        {
            DialogueNodeData dnd = base.Save();
            dnd.inputText = inputText;
            return dnd;
        }
    }

}
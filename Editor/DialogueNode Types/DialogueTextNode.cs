using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


using UnityEditor.UIElements;
namespace DialogueGraphEditor
{
    [Serializable]
    public class DialogueTextNode : DialogueNode, IDisposable
    {
        public string text;
        public string responseText;
        public string characterName;

        public List<Port> outputPorts = new List<Port>();
        Button addPortButton;
        public Sprite sprite;

        public override DialogueNodeType type => DialogueNodeType.Text;
        public override void SetupFromData(DialogueNodeData data)
        {
            title = "Text Node";




            base.SetupFromData(data);
            text = data.text;
            responseText = data.inputText;
            sprite = data.sprite;
            characterName = data.displayName;



            #region layoutBoxes
            VisualElement middleBox = new VisualElement();
            middleBox.name = "columnBox";

            VisualElement fieldRowsBox = new VisualElement();
            fieldRowsBox.name = "columnBox";

            VisualElement middleTopBox = new VisualElement();
            middleTopBox.name = "rowBox";
            #endregion


            #region displayText
            TextFieldWithPlaceholder displayNameTextField = new TextFieldWithPlaceholder("Char's name...", "Name of the character talking");
            displayNameTextField.name = "displayNameTextField";
            displayNameTextField.multiline = true;
            displayNameTextField.RegisterValueChangedCallback(evt =>
            {
                characterName = evt.newValue;
            });
            displayNameTextField.SetValueWithoutNotify(characterName);
            #endregion
            fieldRowsBox.Add(displayNameTextField);
            collapsableVisualElements.Add(displayNameTextField);

            #region spriteField
            ObjectField spriteField = new ObjectField();
            spriteField.name = "valueField";
            spriteField.objectType = typeof(Sprite);

            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>("noneSprite");
            }
            spriteField.value = sprite;
            #endregion
            collapsableVisualElements.Add(spriteField);
            fieldRowsBox.Add(spriteField);

            #region imagePreviewAndImageBck
            Image previewImg = new Image();
            previewImg.name = "previewImg";
            previewImg.image = AssetPreview.GetAssetPreview(sprite);

            VisualElement imageBackground = new VisualElement();
            imageBackground.name = "imageBackground";
            imageBackground.Add(previewImg);
            #endregion
            collapsableVisualElements.Add(imageBackground);

            middleTopBox.Add(fieldRowsBox);
            middleTopBox.Add(imageBackground);

            previewImg.SendToBack();
            // previewImg.BringToFront();


            #region spriteSerializationForField
            SerializedObject serializedSprite = new SerializedObject(sprite);
            spriteField.Bind(serializedSprite);

            spriteField.RegisterValueChangedCallback(x =>
            {
                sprite = x.newValue as Sprite;

                previewImg.image = AssetPreview.GetAssetPreview(sprite);
                previewImg.MarkDirtyRepaint();
            });
            #endregion

            middleBox.Add(middleTopBox);

            #region textField
            TextField textField = new TextFieldWithPlaceholder("Text...", "Text that is displayed");
            textField.name = "textField";

            textField.RegisterValueChangedCallback(evt =>
            {
                text = evt.newValue;
            });
            textField.SetValueWithoutNotify(text);
            textField.multiline = true;
            #endregion
            middleBox.Add(textField);


            topContainer.Add(middleBox);




            #region responseTextField
            TextField responseTextField = new TextFieldWithPlaceholder("Response...", "Response to previous text");
            responseTextField.name = "inputTextField";

            responseTextField.SetValueWithoutNotify(responseText);
            responseTextField.RegisterValueChangedCallback(evt =>
            {
                responseText = evt.newValue;
                foreach (Edge e in inputPort.connections)
                {
                    DialogueGraph.counter = 0;
                    ((DialogueNode)e.output.node).ChangeResponseDisplay(e.output, responseText);
                }
            });
            responseTextField.multiline = true;
            #endregion

            inputContainer.Add(responseTextField);

            //Add port button

            addPortButton = new Button(() => { AddPort(System.Guid.NewGuid().ToString()); })
            {
                text = "Add Response"
            };


            outputContainer.Add(addPortButton);
            addPortButton.BringToFront();

            //Generate ports

            for (int i = 0; i < data.outputGuids.Count; i++)
            {
                AddPort(data.outputGuids[i]);
            }


            //   previewImg.BringToFront();
            Refresh();
        }
        public override bool IsCopiable()
        {
            return true;
        }
        public override bool ChangeResponseDisplay(Port p, string text)
        {
            p.contentContainer.Q<Label>("type").text = text;
            return base.ChangeResponseDisplay(p, text);
        }
        public void AddPort(string id)
        {
            string guid = id;
            Port outputPort = AddPort(Direction.Output, guid);
            Button removePortButton = new Button()
            {
                text = "X",
                userData = guid,
            };
            removePortButton.clicked += () => { RemovePort(outputPorts.FindIndex(x => x.userData == removePortButton.userData)); };

            outputPort.Add(removePortButton);
            outputPorts.Add(outputPort);
            outputContainer.Add(outputPort);
            addPortButton.BringToFront();
        }
        public void RemovePort(int id)
        {

            List<Edge> targetEdges = outputPorts[id].connections.ToList();
            if (targetEdges.Count > 0)
            {
                Edge edge = targetEdges[0];
                edge.input.Disconnect(edge);
                edge.parent.Remove(edge);
            }

            outputContainer.Remove(outputPorts[id]);
            outputPorts.RemoveAt(id);
            Refresh();
        }
        public override Port GetPort(Direction direction, string portData)
        {

            Port p = base.GetPort(direction, portData);
            if (p != null)
            {
                return p;
            }
            foreach (Port port in outputPorts)
            {
                if (ComparePortData(port, direction, portData))
                {
                    return port;
                }
            }

            return null;
        }
        public override DialogueNodeData Save()
        {
            DialogueNodeData dnd = base.Save();

            dnd.text = text;
            List<string> ids = new List<string>();
            foreach (Port port in outputPorts)
            {
                ids.Add((string)port.userData);
            }
            dnd.outputGuids = ids;
            dnd.sprite = sprite;
            dnd.displayName = characterName;
            dnd.inputText = responseText;
            return dnd;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

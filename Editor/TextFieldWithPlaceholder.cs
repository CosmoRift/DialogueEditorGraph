using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TextFieldWithPlaceholder : TextField
{
    Label placeholder;
    public TextFieldWithPlaceholder(string placeholderText, string tooltipInfo)
    {

        placeholder = new Label(placeholderText);
        this.Q("unity-text-input").contentContainer.Add(placeholder);
        this.tooltip = tooltipInfo;
        this.RegisterValueChangedCallback(x =>
        {
            placeholder.visible = x.newValue.Length < 1;
        });
        placeholder.name = "placeholder";
    }
    public override void SetValueWithoutNotify(string newValue)
    {
        base.SetValueWithoutNotify(newValue);
        if (placeholder != null)
        {
            placeholder.visible = newValue.Length < 1;
        }
    }
}

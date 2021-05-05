using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;
using UltEvents;

namespace DialogueGraphEditor
{
    public enum DialogueNodeType { Null, Text, Start, Function, Branch, Exit }
    [Serializable]
    public class DialogueNodeData
    {

        public DialogueNodeType type;

        [AllowNesting, ShowIf("isTextType")] public Sprite sprite = null;
        [AllowNesting, ShowIf("isTextType")] public string text = "";
        [AllowNesting, ShowIf("isTextType")] public string displayName = "";
        [AllowNesting, ShowIf("isTextType")] public string inputText = "";


        [AllowNesting, ShowIf("isExecuteType")] public UltEvent OnExecute = new UltEvent();

        [AllowNesting, ShowIf("isBranchType")] public Condition OnCheck = new Condition();


        [Header("Graph Data. Do not touch")]
        [AllowNesting, EnableIf("isNullType")] public string guid;
        [AllowNesting, EnableIf("isNullType")] public Vector2 position = Vector2.zero;
        [SerializeField]
        [HideInInspector] public List<string> outputGuids = new List<string>();
        /*  string spritePath;

          public void OnBeforeSerialize()
          {
              spritePath = AssetDatabase.GetAssetPath(sprite);


          }

          public void OnAfterDeserialize()
          {
              try
              {
                  sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
              }
              catch (Exception e)
              {
                  //  EditorUtility.DisplayDialog("Error", "There was an error trying to load sprite from path!", "Ok");
              }
          }*/
        #region Checks for editor
        bool isTextType => type == DialogueNodeType.Text;
        bool isStartType => type == DialogueNodeType.Start;
        bool isExecuteType => type == DialogueNodeType.Function;
        bool isBranchType => type == DialogueNodeType.Branch;
        bool isNullType => type == DialogueNodeType.Null;
        #endregion
    }
}

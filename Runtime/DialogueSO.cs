using System.Collections.Generic;
using UnityEngine;
namespace DialogueGraphEditor
{

    [System.Serializable]
    [CreateAssetMenu(fileName = "New DialogueSO", menuName = "Custom/DialogueSO")]
    public class DialogueSO : ScriptableObject
    {
        public List<NodeLinkData> links = new List<NodeLinkData>();

        public List<DialogueNodeData> nodes = new List<DialogueNodeData>() {
    new DialogueNodeData(){
    type = DialogueNodeType.Start,
    guid =System.Guid.NewGuid().ToString(),
    position=Vector2.zero,
    }
    };
        public List<CommentBlockData> comments = new List<CommentBlockData>();
        /// <summary>
        /// Returns node that is connected to choosen output
        /// </summary>
        /// <param name="baseNode">Node that has output port</param>
        /// <param name="outputGuid">Output's port</param>
        /// <returns></returns>
        public DialogueNodeData GetNodeConnectedToOutput(DialogueNodeData baseNode, string outputGuid)
        {
            NodeLinkData nld = links.Find(x => x.outputPortGUID == outputGuid && x.outputNodeGUID == baseNode.guid);
            if (nld == null)
            {
                return null;
            }
            return nodes.Find(x => x.guid == nld.inputNodeGUID);



        }
        /// <summary>
        /// Gets Start node
        /// </summary>
        /// <returns></returns>
        public DialogueNodeData GetStartNode()
        {
            return nodes.Find(x => x.type == DialogueNodeType.Start);
        }

        public bool True()
        {
            return true;
        }
        public bool False()
        {
            return false;
        }
        public bool FiftyFifty()
        {
            return Random.value >= 0.5f;
        }
        public void StealCoins(int number)
        {
            Debug.Log("You stolen " + number + "coins.");


        }
    }
}

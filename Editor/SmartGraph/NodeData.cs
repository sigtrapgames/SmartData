using System.Collections.Generic;
using UnityEngine;

namespace SmartData.Graph
{
    [System.Serializable]
    public class NodeData
    {
		public enum NodeType
		{
			Default,
			SmartData,
			SmartEvent
		}

        public Object Entity { get; private set; }
        public string EntityType {get; private set;}
        public bool IsSmartObject {get; private set;}
		
		public NodeType Behaviour { get; private set; }

        public string Name
        {
            get
            {
                return Entity != null ? Entity.name : "<Missing>";
            }
        }
        
        public List<SmartGraphConnection> Outputs { get; private set; }
        public List<SmartGraphConnection> Inputs { get; private set; }


		public NodeData(Object entity, NodeType type)
		{
			Entity = entity;
			Behaviour = type;
			Outputs = new List<SmartGraphConnection>();
			Inputs = new List<SmartGraphConnection>();
            EntityType = Entity.GetType().Name;
            System.Type tData;
            IsSmartObject = Editors.SmartEditorUtils.GetSmartObjectType(Entity.GetType(), out tData) != Editors.SmartEditorUtils.SmartObjectType.NONE;
		}


		#region registry


		[SerializeField]
		private static Dictionary<int, NodeData> nodes = new Dictionary<int, NodeData>();

		public static ICollection<NodeData> Nodes
        {
            get
            {
                return nodes != null ? nodes.Values : null;
            }
        }

		public static void ClearAll()
		{
			nodes.Clear();
		}
		
		public static void RegisterEvent(SmartGraphConnection eventCall)
        {
			CreateNode(eventCall.Sender);
            CreateNode(eventCall.Receiver);

            nodes[eventCall.Sender.GetInstanceID()].Outputs.Add(eventCall);
            nodes[eventCall.Receiver.GetInstanceID()].Inputs.Add(eventCall);
        }


        private static void CreateNode(Object entity)
        {
			int id = entity.GetInstanceID();

			if (!nodes.ContainsKey(id))
			{ 
				nodes.Add(id, new NodeData(entity, NodeTypeForObject(entity)));
            }
		}

		private static NodeType NodeTypeForObject(Object obj)
		{
			if (obj is SmartData.SmartEvent.Data.EventVar)
			{
				return NodeType.SmartEvent;
			}
			if (obj is SmartData.Abstract.SmartBase)
			{
				return NodeType.SmartData;
			}
			return NodeType.Default;
		}
		#endregion
	}

}
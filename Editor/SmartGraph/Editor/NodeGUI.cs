using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.Graphs;
using System.Collections.Generic;

namespace SmartData.Graph
{
    public class NodeGUI : Node
    {
        #region Public class methods

        // Factory method
        static public NodeGUI Create(NodeData dataInstance)
        {
            var node = CreateInstance<NodeGUI>();
            node.Initialize(dataInstance);
            node.name = dataInstance.Entity.GetInstanceID().ToString();

			System.Type tData;
			node.SmartType = Editors.SmartEditorUtils.GetSmartObjectType(dataInstance.Entity.GetType(), out tData);
			switch (node.SmartType)
			{
				case Editors.SmartEditorUtils.SmartObjectType.CONST: node.color = Styles.Color.Gray; break;
				case Editors.SmartEditorUtils.SmartObjectType.VAR: node.color = Styles.Color.Blue; break;
				case Editors.SmartEditorUtils.SmartObjectType.MULTI: node.color = Styles.Color.Aqua; break;
				case Editors.SmartEditorUtils.SmartObjectType.EVENT_MULTI: node.color = Styles.Color.Aqua; break;
				case Editors.SmartEditorUtils.SmartObjectType.EVENT: node.color = Styles.Color.Orange; break;
				case Editors.SmartEditorUtils.SmartObjectType.SET: node.color = Styles.Color.Yellow; break;
				default: node.color = Styles.Color.Grey; break;
			}

			if (node.SmartType != Editors.SmartEditorUtils.SmartObjectType.NONE)
			{
				node.icon = Editors.SmartEditorUtils.LoadSmartIcon(node.SmartType, true, false);
			}
            else
            {
                node.icon = Editors.SmartEditorUtils.iconGameobject;
            }

			return node;
        }

		#endregion

		#region Public member properties and methods

		private Texture2D icon;

		public Editors.SmartEditorUtils.SmartObjectType SmartType { get; private set; }

		// Runtime instance access
		public NodeData runtimeInstance
        {
            get { return _runtimeInstance; }
        }

        // Validity check
        public bool isValid
        {
            get { return _runtimeInstance != null; }
        }

        #endregion

        #region Overridden virtual methods

        // Node display title
        public override string title
        {
            get { return isValid ? _runtimeInstance.Name : "<Missing>"; }
        }

        // Dirty callback
        public override void Dirty()
        {
            base.Dirty();
        }

		public override void NodeUI(GraphGUI host)
		{
			base.NodeUI(host);
			if (icon != null)
			{
				GUI.DrawTexture(new Rect(Vector2.one*5, new Vector2(20, 20)), icon);
			}
		}
		
		
		#endregion

		#region Private members

		// Runtime instance of this node
		NodeData _runtimeInstance;

        // Initializer (called from the Create method)
        void Initialize(NodeData runtimeInstance)
        {
            hideFlags = HideFlags.DontSave;

            // Object references
            _runtimeInstance = runtimeInstance;
            position = new Rect(Vector2.one * UnityEngine.Random.Range(0, 500), Vector2.zero);

            PopulateSlots();
        }

        void PopulateSlots()
        {
            foreach (SmartGraphConnection call in _runtimeInstance.Outputs)
            {
                string name = call.EventName;
                string title = ObjectNames.NicifyVariableName(name);
                if (!outputSlots.Any(s => s.title == title))
                {
                    var slot = AddOutputSlot(name);
                    slot.title = title;
                }
            }

            // Ensure Read-Only then Read/Write
            if (_runtimeInstance.Inputs.Count > 1 && _runtimeInstance.Inputs[0].MethodFullPath.ToLower().Contains("write"))
            {
                var i = _runtimeInstance.Inputs[0];
                _runtimeInstance.Inputs[0] = _runtimeInstance.Inputs[1];
                _runtimeInstance.Inputs[1] = i;
            }

            foreach (SmartGraphConnection call in _runtimeInstance.Inputs)
            {
                string name = call.MethodFullPath;
                string title = ObjectNames.NicifyVariableName(name);
                if (!inputSlots.Any(s => s.title == title))
                {
                    var slot = AddInputSlot(name);
                    slot.title = title;
                }
            }
        }
		
        public void PopulateEdges()
        {
            foreach (var outSlot in outputSlots)
            {
                List<SmartGraphConnection> outCalls = _runtimeInstance.Outputs.FindAll(call => call.EventName == outSlot.name);

                foreach (SmartGraphConnection call in outCalls)
                {
                    var targetNode = graph[call.Receiver.GetInstanceID().ToString()];
                    var inSlot = targetNode[call.MethodFullPath];

                    if (!graph.Connected(outSlot, inSlot))
                    {
                        Edge edge = graph.Connect(outSlot, inSlot);
						EdgeGUI.EdgesData.Add(edge, call);

                        call.OnTriggered += ((direction) =>
						{
							EdgeTriggersTracker.RegisterTrigger(edge, direction);
							if (direction)
							{
								foreach(var otherEdges in inSlot.node.inputEdges)
								{
									if(otherEdges != edge)
									{
										EdgeTriggersTracker.RegisterTrigger(otherEdges, !direction);
									}
								}
							}
						});
                    }
                }
            }
        }

        #endregion
    }
}

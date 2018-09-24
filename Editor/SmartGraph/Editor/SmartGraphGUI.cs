using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;

namespace SmartData.Graph
{
    [System.Serializable]
	public class SmartGraphGUI : GraphGUI
    {
        [SerializeField]
        public int SelectionOverride;
		
		public new float zoomLevel
		{
			get;set;
		}

		public override void OnGraphGUI()
        {
			
			// Show node subwindows.
			m_Host.BeginWindows();
			
			foreach (var node in graph.nodes)
            {
                // Recapture the variable for the delegate.
                NodeGUI node2 = node as NodeGUI;

                // Subwindow style (active/nonactive)
                var isActive = selection.Contains(node2);
				
                var style = Styles.GetNodeStyle(node2.style, node2.color, isActive);
                var newStyle = new GUIStyle(style);
                newStyle.fontSize = 12;

				node2.position = GUILayout.Window(
                    node.GetInstanceID(), 
                    node.position,
                    delegate { NodeGUI(node2); },
                    node.title, 
                    newStyle, 
                    GUILayout.MinWidth(200)
                );
				
            }
            
            if (graph.nodes.Count == 0)
            { 
                GUILayout.Window(0, new Rect(0, 0, 1, 1), delegate {}, "", "MiniLabel");
            }
			
			m_Host.EndWindows();

			// Graph edges
			edgeGUI.DoEdges();

			
			// Mouse drag
#if UNITY_2017 || UNITY_2017_1_OR_NEWER
			DragSelection();
#else
			DragSelection(new Rect(-5000, -5000, 10000, 10000));
#endif
			
		}
		
        
        public override IEdgeGUI edgeGUI
        {
            get
            {
                if (m_EdgeGUI == null)
                    m_EdgeGUI = new EdgeGUI { host = this };
                return m_EdgeGUI;
            }
        }
		

        public override void NodeGUI(Node node)
        {
            NodeGUI ng = node as NodeGUI;
            if (ng.runtimeInstance.IsSmartObject)
            {
                Color gcc = GUI.contentColor;
                GUI.contentColor = Color.black;
                GUILayout.Label(ng.runtimeInstance.EntityType);
                GUI.contentColor = gcc;
            }
            
            SelectNode(node);
            GUIStyle pinIn = new GUIStyle(Styles.triggerPinIn);
            GUIStyle pinOut = new GUIStyle(Styles.triggerPinOut);
            pinIn.fontSize = pinOut.fontSize = 11;
            GUIStyle noteIn = new GUIStyle();
            noteIn.fontSize = 10;
            GUIStyle noteOut = new GUIStyle(noteIn);
            noteOut.alignment = TextAnchor.MiddleRight;
            

            foreach (var slot in node.inputSlots)
			{
				LayoutSlot(slot, slot.title, false, true, true, pinIn);
                GUILayout.Space(4); // Bottom padding
			}

            node.NodeUI(this);

            int i=0;
            foreach (var slot in node.outputSlots)
            {
                bool hasNotes = !string.IsNullOrEmpty(ng.runtimeInstance.Outputs[i].Notes);

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5); // Minimum left padding
                    
                    if (hasNotes){
                        ng.runtimeInstance.Outputs[i].ShowNotes = EditorGUILayout.Foldout(ng.runtimeInstance.Outputs[i].ShowNotes, "");
                    } else {
                        GUILayout.FlexibleSpace();
                    }
                    
                    LayoutSlot(slot, slot.title, true, false, true, pinOut);
                }
                EditorGUILayout.EndHorizontal();

                // Draw notes
                if (hasNotes && ng.runtimeInstance.Outputs[i].ShowNotes){
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(18);
                    GUILayout.Label(ng.runtimeInstance.Outputs[i].Notes, noteOut);
                    GUILayout.Space(5);
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(4); // Bottom padding

                ++i;
			}

            GUILayout.Space(1);

            var description = ng.runtimeInstance.Description;
            if (!string.IsNullOrEmpty(description))
            {
                ng.runtimeInstance.ShowDescription = EditorGUILayout.Foldout(ng.runtimeInstance.ShowDescription, ng.runtimeInstance.IsSmartObject ? "Description" : "Notes");
                if (ng.runtimeInstance.ShowDescription)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(18);
                    GUILayout.Label(description, noteIn);
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.Space(1);
            }

			DragNodes();

            UpdateSelection();
        }

        private void UpdateSelection()
        {
            OverrideSelection();
            if (selection.Count > 0)
            {
                int[] selectedIds = new int[selection.Count];
                for (int i = 0; i < selection.Count; i++)
                {
                    selectedIds[i] = int.Parse(selection[i].name);
                }
                Selection.instanceIDs = selectedIds;
            }
        }

        private void OverrideSelection()
        {
            if (SelectionOverride != 0)
            {
                Node selectedNode = graph[SelectionOverride.ToString()];
                if (selectedNode != null)
                {
                    selection.Clear();
                    selection.Add(selectedNode);
                    CenterGraph(selectedNode.position.position);
                    
                }
                SelectionOverride = 0;
            }
        }
             
        
        

    }
}
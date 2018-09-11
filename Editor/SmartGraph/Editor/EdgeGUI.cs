//
// Klak - Utilities for creative coding with Unity
//
// Copyright (C) 2016 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;
using Graphs = UnityEditor.Graphs;
using UnityEditor.Graphs;

namespace SmartData.Graph
{
	// Specialized edge drawer class
	public class EdgeGUI : Graphs.IEdgeGUI
	{//TODO move away
		public static Dictionary<Edge, SmartGraphConnection> EdgesData = new Dictionary<Edge, SmartGraphConnection>();
		#region Public members

		private SmartGraphGUI _host;
		public Graphs.GraphGUI host
		{
			get
			{
				return _host;
			}
			set
			{
				_host = value as SmartGraphGUI;
			}
		}
		public List<int> edgeSelection { get; set; }

		public EdgeGUI()
		{
			edgeSelection = new List<int>();
		}

		#endregion

		#region IEdgeGUI implementation


		public void DoEdges()
		{
			// Draw edges on repaint.
			if (Event.current.type == EventType.Repaint)
			{
				foreach (var edge in host.graph.edges)
				{
					//if (edge == _moveEdge) continue;

					Vector2Int indexes = FindSlotIndexes(edge);

					bool isReadOnly = false;
					if (EdgesData.ContainsKey(edge))
					{
						isReadOnly = EdgesData[edge].ReadOnly;
					}
					DrawEdge(edge,
						indexes,
						ColorForIndex(edge.toSlot.node as NodeGUI),
						Mathf.Lerp(5, 1, _host.zoomLevel),
						isReadOnly);
				}
			}
		}
		private Vector2Int FindSlotIndexes(Edge edge)
		{
			Vector2Int indexes = Vector2Int.zero;

			int totalOutputs = 0;
			bool found = false;
			foreach (var slot in edge.fromSlot.node.outputSlots)
			{
				if (slot != edge.fromSlot && !found)
				{
					indexes.x++;
				}
				else
				{
					found = true;
				}
				totalOutputs++;
			}
			indexes.x = totalOutputs - indexes.x - 1;

			foreach (var slot in edge.toSlot.node.inputSlots)
			{
				if (slot != edge.toSlot)
				{
					indexes.y++;
				}
				else
				{
					break;
				}
			}

			return indexes;
		}

		private Color ColorForIndex(NodeGUI node)
		{
			Color colour = Color.white;
			if (node.SmartType != Editors.SmartEditorUtils.SmartObjectType.NONE)
			{
				colour = Editors.SmartEditorUtils.GetSmartColor(node.SmartType);
			}
			
			return colour ;
		}

		public void DoDraggedEdge()
		{

		}

		public void BeginSlotDragging(Graphs.Slot slot, bool allowStartDrag, bool allowEndDrag)
		{

		}

		public void SlotDragging(Graphs.Slot slot, bool allowEndDrag, bool allowMultiple)
		{

		}

		public void EndSlotDragging(Graphs.Slot slot, bool allowMultiple)
		{

		}


		public void EndDragging()
		{

		}

		public Graphs.Edge FindClosestEdge()
		{
			return null;
		}


		#endregion

		#region Private members

		//Graphs.Edge _moveEdge;
		Graphs.Slot _dragSourceSlot;
		Graphs.Slot _dropTarget;

		#endregion

		#region Edge drawer

		const int kUnitsPerDash = 5;
		const float kEdgeWidth = 6;
		const float kNodeTitleSpace = 56;
		const float kNodeEdgeSeparation = 16;



		static void DrawEdge(Edge edge, Vector2Int indexes, Color color, float thicknessFactor, bool dashed)
		{
			var p1 = GetPositionAsFromSlot(edge.fromSlot, indexes.x);
			var p2 = GetPositionAsToSlot(edge.toSlot, indexes.y);
			DrawEdge(p1, p2, color * edge.color, thicknessFactor, EdgeTriggersTracker.GetTimings(edge), dashed);
		}

		static Vector3[] _bezierPoints = new Vector3[2];
		static Vector3 GetBezierPoint(Vector2 start, Vector3 end, Vector3 handle1, Vector3 handle2, float t)
		{
			float u = 1f - t;
			Vector3 result = start * u * u * u;
			result += (3 * u * u * t * handle1);
			result += (3 * u * t * t * handle2);
			result += (t * t * t * end);
			return result;
		}
		static void DrawEdge(Vector2 p1, Vector2 p2, Color color, float thicknessFactor, List<float> triggers, bool dashed)
		{
			Color prevColor = Handles.color;
			Handles.color = color;
			float thickness = kEdgeWidth * thicknessFactor;
			var l = Mathf.Min(Mathf.Abs(p1.y - p2.y), 50);
			Vector2 p3 = p1 + new Vector2(l, 0);
			Vector2 p4 = p2 - new Vector2(l, 0);
			var linetex = (Texture2D)UnityEditor.Graphs.Styles.selectedConnectionTexture.image;

			if (dashed)
			{
				// dashes per unit
				float distance = Vector3.Distance(p1, p2);
				int dashes = Mathf.FloorToInt(distance / kUnitsPerDash);
				if (dashes % 2 != 0) ++dashes;

				for (int i = 0; i < dashes; i += 2)
				{
					// Get bezier point
					_bezierPoints[0] = GetBezierPoint(p1, p2, p3, p4, (float)i / (float)dashes);
					_bezierPoints[1] = GetBezierPoint(p1, p2, p3, p4, (float)(i + 1) / (float)dashes);

					Handles.DrawAAPolyLine(linetex, thickness, _bezierPoints);
				}
			}
			else
			{
				Handles.DrawBezier(p1, p2, p3, p4, color, linetex, thickness);
			}

			foreach (var trigger in triggers)
			{
				Vector3 pos = CalculateBezierPoint(trigger, p1, p3, p4, p2);
				Handles.DrawSolidArc(pos, Vector3.back, pos + Vector3.up, 360, thickness);

			}

			Handles.color = prevColor;
		}

		#endregion

		#region Utilities to access private members

		static Vector2 GetPositionAsFromSlot(Slot slot, int index)
		{
			NodeGUI node = slot.node as NodeGUI;
			Vector2 pos = node.position.position;
			pos.y = node.position.yMax - kNodeEdgeSeparation * 0.5f;
			pos.y -= kNodeEdgeSeparation * index;
			pos.x = node.position.xMax;

			return pos;
		}

		static Vector2 GetPositionAsToSlot(Slot slot, int index)
		{
			NodeGUI node = slot.node as NodeGUI;
			Vector2 pos = node.position.position;
			pos.y += kNodeTitleSpace;
			pos.y += kNodeEdgeSeparation * index;
			pos.x = node.position.x;

			return pos;
		}

		#endregion

		private static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			float u = 1.0f - t;
			float tt = t * t;
			float uu = u * u;
			float uuu = uu * u;
			float ttt = tt * t;

			Vector3 p = uuu * p0;
			p += 3 * uu * t * p1;
			p += 3 * u * tt * p2;
			p += ttt * p3;

			return p;
		}
	}

}

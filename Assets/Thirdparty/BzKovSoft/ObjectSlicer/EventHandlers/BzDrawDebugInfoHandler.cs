using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.EventHandlers
{
	/// <summary>
	/// The component will draw a debug information
	/// </summary>
	[DisallowMultipleComponent]
	public class BzDrawDebugInfoHandler : MonoBehaviour, IBzObjectSlicedEvent
	{
		Stopwatch _stopwatch;
		int _vertexCount;
		Queue<Matrix4x4> _lastPlaneTrs = new Queue<Matrix4x4>();
		static string _drawText = "-";

		[SerializeField]
		bool drawCenterOfMass;
		[SerializeField]
		bool drawLastPlane;

		public bool OnSlice(IBzMeshSlicer meshSlicer, Plane plane, object sliceData)
		{
			Quaternion rotation = Quaternion.LookRotation(plane.normal);
			_lastPlaneTrs.Enqueue(Matrix4x4.TRS(plane.ClosestPointOnPlane(transform.position), rotation, Vector3.one));
			if (_lastPlaneTrs.Count > 5)
			{
				_lastPlaneTrs.Dequeue();
			}

			_stopwatch = Stopwatch.StartNew();
			var filters = GetComponentsInChildren<MeshFilter>();
			int vertexCount = 0;
			for (int i = 0; i < filters.Length; i++)
			{
				vertexCount += filters[i].sharedMesh.vertexCount;
			}
			_vertexCount = vertexCount;

			return true;
		}

		public void ObjectSliced(GameObject original, GameObject[] resultObjects, BzSliceTryResult result, object sliceData)
		{
			_stopwatch.Stop();
			_drawText += gameObject.name +
				". VertCount: " + _vertexCount.ToString() + ". ms: " +
				_stopwatch.ElapsedMilliseconds.ToString() + Environment.NewLine;

			if (_drawText.Length > 1500) // prevent very long text
			{
				_drawText = _drawText.Substring(_drawText.Length - 1000, 1000);
			}
		}

		void OnDrawGizmosSelected()
		{
			if (drawCenterOfMass)
			{
				Rigidbody rigid = this.GetComponent<Rigidbody>();
				if (rigid == null)
					return;

				Vector3 pos = this.transform.position + this.transform.TransformDirection(rigid.centerOfMass);
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(pos, 0.1f);
			}

			if (drawLastPlane)
			{
				Color32 color = Color.blue;
				color.a = 125;
				Gizmos.color = color;
				foreach (var trs in _lastPlaneTrs)
				{
					Gizmos.matrix = trs;
					Gizmos.DrawCube(Vector3.zero, new Vector3(1.0f, 1.0f, 0.0001f));
				}
				Gizmos.matrix = Matrix4x4.identity;
				Gizmos.color = Color.white;
			}
		}

		void OnGUI()
		{
			GUI.Label(new Rect(10, 10, 2000, 2000), _drawText);
		}
	}
}

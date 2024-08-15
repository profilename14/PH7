using System;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.Tests
{
	/// <summary>
	/// Manual mesh adapter with a default settings
	/// </summary>
	public class BzManualMeshAdapter : IBzSliceAdapter
	{
		public Vector3 GetFixedPos(BzMeshData meshData, int index)
		{
			return meshData.Vertices[index];
		}

		public Vector3 GetWorldPos(BzMeshData meshData, int index)
		{
			return meshData.Vertices[index];
		}

		public Vector3 InverseTransformDirection(Vector3 p)
		{
			return p;
		}

		public void RebuildMesh(Mesh mesh, Material[] materials, Renderer meshRenderer)
		{
			throw new NotSupportedException();
		}

		public Vector3 GetObjectCenterInWorldSpace()
		{
			return Vector3.zero;
		}
	}
}
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Mesh adapter
	/// </summary>
	public interface IBzSliceAdapter
	{
		/// <summary>
		/// Fixed position for the Polygon Generator
		/// </summary>
		Vector3 GetFixedPos(BzMeshData meshData, int index);
		Vector3 GetWorldPos(BzMeshData meshData, int index);
		Vector3 InverseTransformDirection(Vector3 p);
		void RebuildMesh(Mesh mesh, Material[] materials, Renderer meshRenderer);
		Vector3 GetObjectCenterInWorldSpace();
	}
}
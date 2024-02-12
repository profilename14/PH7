using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Adapter designed for a simple static mesh MeshFilter component
	/// </summary>
	public class BzSliceMeshFilterAdapter : IBzSliceAdapter
	{
		Matrix4x4 _l2w;
		Matrix4x4 _w2l;

		public BzSliceMeshFilterAdapter(Transform transform)
		{
			_l2w = transform.localToWorldMatrix;
			_w2l = transform.worldToLocalMatrix;
		}

		public Vector3 GetFixedPos(BzMeshData meshData, int index)
		{
			return meshData.Vertices[index];
		}

		public Vector3 GetWorldPos(BzMeshData meshData, int index)
		{
			return _l2w.MultiplyPoint3x4(meshData.Vertices[index]);
		}

		public Vector3 InverseTransformDirection(Vector3 p)
		{
			return _w2l.MultiplyPoint3x4(p + _l2w.MultiplyPoint3x4(Vector3.zero));
		}

		public void RebuildMesh(Mesh mesh, Material[] materials, Renderer meshRenderer)
		{
			var meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
			meshFilter.mesh = mesh;
			meshRenderer.sharedMaterials = materials;
		}

		public Vector3 GetObjectCenterInWorldSpace()
		{
			return _l2w.MultiplyPoint3x4(Vector3.zero);
		}
	}
}
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Slicer result
	/// </summary>
	public class BzSliceTryResult
	{
		public bool sliced;
		public Plane plane;
		public BzSlicerTryResultObject[] resultObjects;
		public string rejectMessage;
	}

	/// <summary>
	/// One part of the mesh slicer result
	/// </summary>
	public class BzSlicerTryResultObject
	{
		public GameObject gameObject;
		public bool side;
		public BzMeshSliceResult[] meshes;
	}

	/// <summary>
	/// The mesh of the slicer result
	/// </summary>
	public class BzMeshSliceResult
	{
		public SliceType sliceType;
		/// <summary>
		/// Cut edges
		/// </summary>
		public BzSliceEdgeResult[] sliceEdges;
		public Renderer renderer;
	}

	/// <summary>
	/// Slice cut edge details
	/// </summary>
	public class BzSliceEdgeResult
	{
		public Vector3[] vertices;
		public Vector3[] normals;
		public BoneWeight[] boneWeights;
	}
}
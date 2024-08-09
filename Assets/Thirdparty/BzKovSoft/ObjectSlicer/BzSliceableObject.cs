using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Base class for sliceable object with a static mesh
	/// </summary>
	public class BzSliceableObject : BzSliceableBase
	{
		protected override AdapterAndMesh GetAdapterAndMesh(Renderer renderer)
		{
			var meshRenderer = renderer as MeshRenderer;

			if (meshRenderer != null)
			{
				var result = new AdapterAndMesh();
				result.mesh = meshRenderer.gameObject.GetComponent<MeshFilter>().sharedMesh;
				result.adapter = new BzSliceMeshFilterAdapter(renderer.transform);
				return result;
			}

			return null;
		}
	}
}
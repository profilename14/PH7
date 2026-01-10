using UnityEngine;

namespace BzKovSoft.ObjectSlicer.Samples
{
	/// <summary>
	/// Direct mesh slice
	/// </summary>
	public class SampleManualSlicer : MonoBehaviour
	{
		public GameObject _target;

		void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				// prepare data
				var meshFilter = _target.GetComponent<MeshFilter>();
				var meshRenderer = _target.GetComponent<MeshRenderer>();
				var mesh = meshFilter.mesh;
				Plane plane = new Plane(new Vector3(2f, 1f, 10f).normalized, 0f);
				Material[] materials = meshRenderer.sharedMaterials;
				Material sectionMaterial = new Material(Shader.Find("Diffuse"));
				var adapter = new BzSliceMeshFilterAdapter(_target.transform);

				// slice mesh
				var meshDissector = new BzMeshDataDissector(mesh, plane, materials, adapter, BzSliceConfiguration.GetDefault(), true);
				meshDissector.DefaultSliceMaterial = sectionMaterial;
				SliceResult sliceResult = meshDissector.Slice();

				// apply result back to our object
				if (sliceResult == SliceResult.Sliced)
				{
					var meshData = meshDissector.MeshData;
					var meshItemData = meshData.Meshes[5];
					if (!meshItemData.side)
					{
						meshItemData = meshData.Meshes[4];
					}
					var generatedMesh = meshData.GenerateMeshes(meshItemData);
					meshFilter.mesh = generatedMesh.mesh;
					meshRenderer.materials = generatedMesh.materials;
				}
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Mesh data
	/// </summary>
	public class BzMeshData
	{
		public List<Vector3> WorldVertices;
		public List<Vector3> Vertices;
		public List<Vector3> Normals;

		public List<Color> Colors;
		public List<Color32> Colors32;

		public List<Vector2> UV;
		public List<Vector2> UV2;
		public List<Vector2> UV3;
		public List<Vector2> UV4;
		public List<Vector4> Tangents;

		public List<BoneWeight> BoneWeights;
		public readonly Matrix4x4[] Bindposes;

		public List<BzTriangle> Triangles;
		public int SubMeshCount;
		public List<BzMeshItemData> Meshes;

		public Material[] Materials;

		public bool NormalsExists { get { return Normals != null; } }
		public bool ColorsExists { get { return Colors != null; } }
		public bool Colors32Exists { get { return Colors32 != null; } }
		public bool UVExists { get { return UV != null; } }
		public bool UV2Exists { get { return UV2 != null; } }
		public bool UV3Exists { get { return UV3 != null; } }
		public bool UV4Exists { get { return UV4 != null; } }
		public bool TangentsExists { get { return Tangents != null; } }
		public bool BoneWeightsExists { get { return BoneWeights != null; } }
		public bool MaterialsExists { get { return Materials != null; } }

		public BzMeshData(Mesh initFrom, Material[] materials)
		{
			Materials = materials;
			int vertCount = initFrom.vertexCount / 3;
			Bindposes = initFrom.bindposes;
			if (Bindposes.Length == 0) Bindposes = null;

			Vertices = new List<Vector3>(vertCount);
			Normals = new List<Vector3>(vertCount);
			Colors = new List<Color>();
			Colors32 = new List<Color32>();
			UV = new List<Vector2>(vertCount);
			UV2 = new List<Vector2>();
			UV3 = new List<Vector2>();
			UV4 = new List<Vector2>();
			Tangents = new List<Vector4>();
			BoneWeights = new List<BoneWeight>(Bindposes == null ? 0 : vertCount);
			Triangles = new List<BzTriangle>(vertCount);  // triangle count and vertex count is different, but somewhat close to each other
			SubMeshCount = initFrom.subMeshCount;


			initFrom.GetVertices(Vertices);
			initFrom.GetNormals(Normals);
			initFrom.GetColors(Colors);
			initFrom.GetColors(Colors32);
			initFrom.GetUVs(0, UV);
			initFrom.GetUVs(1, UV2);
			initFrom.GetUVs(2, UV3);
			initFrom.GetUVs(3, UV4);
			initFrom.GetTangents(Tangents);
			initFrom.GetBoneWeights(BoneWeights);

			if (Normals.Count == 0)     Normals = null;
			if (Colors.Count == 0)      Colors = null;
			if (Colors32.Count == 0)    Colors32 = null;
			if (UV.Count == 0)          UV = null;
			if (UV2.Count == 0)         UV2 = null;
			if (UV3.Count == 0)         UV3 = null;
			if (UV4.Count == 0)         UV4 = null;
			if (Tangents.Count == 0)    Tangents = null;
			if (BoneWeights.Count == 0) BoneWeights = null;
		}

		/// <summary>
		/// Generate Unity Mesh object
		/// </summary>
		public BzGeneratedMesh GenerateMeshes(BzMeshItemData meshItemData)
		{
			var materials = Materials == null ? null : new List<Material>();
			var subMeshes = new List<int[]>(SubMeshCount);
			var useMap = new bool[Vertices.Count];
			for (int subMeshIndex = 0; subMeshIndex < SubMeshCount; subMeshIndex++)
			{
				var trs = meshItemData.triangles
					.Where(_ => _.subMeshIndex == subMeshIndex)
					.ToArray();

				if (trs.Length == 0)
				{
					continue;
				}

				materials?.Add(Materials[subMeshIndex]);
				var subMesh = new int[trs.Length * 3];
				subMeshes.Add(subMesh);

				for (int i = 0; i < trs.Length; i++)
				{
					var tr = trs[i];
					int index = i * 3;
					subMesh[index + 0] = tr.i1;
					subMesh[index + 1] = tr.i2;
					subMesh[index + 2] = tr.i3;
					useMap[tr.i1] = true;
					useMap[tr.i2] = true;
					useMap[tr.i3] = true;
				}
			}

			int newSize = 0;
			int[] shiftMap = new int[useMap.Length];
			for (int i = 0; i < useMap.Length; i++)
			{
				bool used = useMap[i];
				shiftMap[i] = newSize;
				if (used)
					++newSize;
			}

			for (int subMeshIndex = 0; subMeshIndex < subMeshes.Count; subMeshIndex++)
			{
				var trs = subMeshes[subMeshIndex];
				for (int i = 0; i < trs.Length; i++)
				{
					int index = trs[i];
					index = shiftMap[index];
					trs[i] = index;
				}
			}

			Mesh mesh = new Mesh();

			if (Vertices.Count > ushort.MaxValue)
			{
				mesh.indexFormat = IndexFormat.UInt32;
			}

			mesh.SetVertices(ReduceSizeByUseMap(Vertices, useMap, newSize));
			if (NormalsExists)
				mesh.SetNormals(ReduceSizeByUseMap(Normals, useMap, newSize));

			if (ColorsExists)
				mesh.SetColors(ReduceSizeByUseMap(Colors, useMap, newSize));
			if (Colors32Exists)
				mesh.SetColors(ReduceSizeByUseMap(Colors32, useMap, newSize));

			if (UVExists)
				mesh.SetUVs(0, ReduceSizeByUseMap(UV, useMap, newSize));
			if (UV2Exists)
				mesh.SetUVs(1, ReduceSizeByUseMap(UV2, useMap, newSize));
			if (UV3Exists)
				mesh.SetUVs(2, ReduceSizeByUseMap(UV3, useMap, newSize));
			if (UV4Exists)
				mesh.SetUVs(3, ReduceSizeByUseMap(UV4, useMap, newSize));

			if (TangentsExists)
				mesh.SetTangents(ReduceSizeByUseMap(Tangents, useMap, newSize));

			if (BoneWeightsExists)
			{
				mesh.boneWeights = ReduceSizeByUseMap(BoneWeights, useMap, newSize);
				mesh.bindposes = Bindposes;
			}

			mesh.subMeshCount = subMeshes.Count;
			for (int i = 0; i < subMeshes.Count; i++)
			{
				mesh.SetTriangles(subMeshes[i], i);
			}

			var result = new BzGeneratedMesh
			{
				mesh = mesh,
				materials = materials?.ToArray(),
			};
			return result;
		}

		private T[] ReduceSizeByUseMap<T>(List<T> vertices, bool[] useMap, int newSize)
		{
			T[] result = new T[newSize];
			int counter = 0;
			for (int i = 0; i < useMap.Length; i++)
			{
				if (useMap[i])
				{
					result[counter++] = vertices[i];
				}
			}

			if (counter != newSize)
			{
				throw new InvalidOperationException("Wrong counter and newSize");
			}

			return result;
		}
	}

	/// <summary>
	/// Generated Mesh and its materials
	/// </summary>
	public class BzGeneratedMesh
	{
		public Mesh mesh;
		public Material[] materials;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using BzKovSoft.ObjectSlicer.Polygon;
using UnityEngine;
using UnityEngine.Profiling;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Mesh data editor
	/// </summary>
	public class BzMeshDataEditor
	{
		private const float MinPointDistanceSqr = 0.00001f;
		public List<IndexVector> CapEdges = new List<IndexVector>();
		readonly IBzSliceAdapter _adapter;
		readonly bool _skipIfNotClosed;
		readonly BzMeshData _meshData;

		readonly Plane _plane;
		readonly Dictionary<IndexVector, int> _chashedOuterLines;

		public BzMeshDataEditor(BzMeshData meshData, Plane plane, IBzSliceAdapter adapter, bool skipIfNotClosed)
		{
			_meshData = meshData;
			_adapter = adapter;
			_skipIfNotClosed = skipIfNotClosed;
			_plane = plane;
			_chashedOuterLines = new Dictionary<IndexVector, int>();
		}

		/// <summary>
		/// Cut this triangle by the plane
		/// </summary>
		public void DivideByPlane(int i1, int i2, int i3, List<BzTriangle> triangles, int subMeshIndex, bool side1, bool side2, bool side3)
		{
			if (!side1 & side2 & side3)
			{
				DivideTriangle(i1, i2, i3, triangles, subMeshIndex, true);
			}
			else if (side1 & !side2 & side3)
			{
				DivideTriangle(i2, i3, i1, triangles, subMeshIndex, true);
			}
			else if (side1 & side2 & !side3)
			{
				DivideTriangle(i3, i1, i2, triangles, subMeshIndex, true);
			}


			else if (side1 & !side2 & !side3)
			{
				DivideTriangle(i1, i2, i3, triangles, subMeshIndex, false);
			}
			else if (!side1 & side2 & !side3)
			{
				DivideTriangle(i2, i3, i1, triangles, subMeshIndex, false);
			}
			else if (!side1 & !side2 & side3)
			{
				DivideTriangle(i3, i1, i2, triangles, subMeshIndex, false);
			}
			else
				throw new InvalidOperationException();
		}

		/// <summary>
		/// Generate new index in triangle list that resides in plane
		/// </summary>
		private int GetIndexFor(int from, int to)
		{
			var oldDir = new IndexVector(from, to);
			int result;
			if (_chashedOuterLines.TryGetValue(oldDir, out result))
				return result;

			Vector3 v1 = _adapter.GetWorldPos(_meshData, from);
			Vector3 v2 = _adapter.GetWorldPos(_meshData, to);
			Vector3 enter;
			float ratioIn = CalcGetRatio(v1, v2, out enter);

			if ((enter - v1).sqrMagnitude < float.Epsilon)
				return from;

			if ((enter - v2).sqrMagnitude < float.Epsilon)
				return to;

			result = _meshData.Vertices.Count;
			_chashedOuterLines.Add(oldDir, result);

			{ // Vertex
				Vector3 vFrom = _meshData.Vertices[from];
				Vector3 vTo = _meshData.Vertices[to];
				Vector3 vNew = Vector3.Lerp(vFrom, vTo, ratioIn);
				_meshData.Vertices.Add(vNew);
			}

			{ // WorldVertex
				Vector3 vNew = Vector3.Lerp(v1, v2, ratioIn);
				_meshData.WorldVertices.Add(vNew);
			}

			if (_meshData.NormalsExists)
			{
				Vector3 nFrom = _meshData.Normals[from];
				Vector3 nTo = _meshData.Normals[to];
				Vector3 nNew = Vector3.Slerp(nFrom, nTo, ratioIn);
				_meshData.Normals.Add(nNew);
			}

			if (_meshData.ColorsExists)
			{
				Color colorFrom = _meshData.Colors[from];
				Color colorTo = _meshData.Colors[to];
				Color colorNew = Color.Lerp(colorFrom, colorTo, ratioIn);
				_meshData.Colors.Add(colorNew);
			}
			if (_meshData.Colors32Exists)
			{
				Color32 colorFrom = _meshData.Colors32[from];
				Color32 colorTo = _meshData.Colors32[to];
				Color32 colorNew = Color32.Lerp(colorFrom, colorTo, ratioIn);
				_meshData.Colors32.Add(colorNew);
			}

			if (_meshData.UVExists)
			{
				AddValue(_meshData.UV, from, to, ratioIn);
			}
			if (_meshData.UV2Exists)
			{
				AddValue(_meshData.UV2, from, to, ratioIn);
			}
			if (_meshData.UV3Exists)
			{
				AddValue(_meshData.UV3, from, to, ratioIn);
			}
			if (_meshData.UV4Exists)
			{
				AddValue(_meshData.UV4, from, to, ratioIn);
			}

			if (_meshData.TangentsExists)
			{
				Vector4 vFrom = _meshData.Tangents[from];
				Vector4 vTo = _meshData.Tangents[to];
				Vector4 vNew = Vector4.Lerp(vFrom, vTo, ratioIn);
				_meshData.Tangents.Add(vNew);
			}

			if (_meshData.BoneWeightsExists)
			{
				var w1 = _meshData.BoneWeights[from];
				var w2 = _meshData.BoneWeights[to];

				var ws = new Dictionary<int, float>(8);
				float ratioOut = 1 - ratioIn;

				float tmp;
				ws.TryGetValue(w1.boneIndex0, out tmp);
				ws[w1.boneIndex0] = tmp + w1.weight0 * ratioIn;
				ws.TryGetValue(w1.boneIndex1, out tmp);
				ws[w1.boneIndex1] = tmp + w1.weight1 * ratioIn;
				ws.TryGetValue(w1.boneIndex2, out tmp);
				ws[w1.boneIndex2] = tmp + w1.weight2 * ratioIn;
				ws.TryGetValue(w1.boneIndex3, out tmp);
				ws[w1.boneIndex3] = tmp + w1.weight3 * ratioIn;

				ws.TryGetValue(w2.boneIndex0, out tmp);
				ws[w2.boneIndex0] = tmp + w2.weight0 * ratioOut;
				ws.TryGetValue(w2.boneIndex1, out tmp);
				ws[w2.boneIndex1] = tmp + w2.weight1 * ratioOut;
				ws.TryGetValue(w2.boneIndex2, out tmp);
				ws[w2.boneIndex2] = tmp + w2.weight2 * ratioOut;
				ws.TryGetValue(w2.boneIndex3, out tmp);
				ws[w2.boneIndex3] = tmp + w2.weight3 * ratioOut;

				var wsArr = ws
					.OrderByDescending(v => v.Value)
					.Take(4)
					.ToArray();

				KeyValuePair<int, float>[] wsArr4 = new KeyValuePair<int, float>[4];
				Array.Copy(wsArr, wsArr4, wsArr.Length);

				float weightSum = 0;
				weightSum += wsArr4[0].Value;
				weightSum += wsArr4[1].Value;
				weightSum += wsArr4[2].Value;
				weightSum += wsArr4[3].Value;

				var newBoneWeight = new BoneWeight();
				float weightRatio = 1 / weightSum;

				if (wsArr.Length > 0)
				{
					newBoneWeight.boneIndex0 = wsArr[0].Key;
					newBoneWeight.weight0 = wsArr[0].Value * weightRatio;
				}
				if (wsArr.Length > 1)
				{
					newBoneWeight.boneIndex1 = wsArr[1].Key;
					newBoneWeight.weight1 = wsArr[1].Value * weightRatio;
				}
				if (wsArr.Length > 2)
				{
					newBoneWeight.boneIndex2 = wsArr[2].Key;
					newBoneWeight.weight2 = wsArr[2].Value * weightRatio;
				}
				if (wsArr.Length > 3)
				{
					newBoneWeight.boneIndex3 = wsArr[3].Key;
					newBoneWeight.weight3 = wsArr[3].Value * weightRatio;
				}

				_meshData.BoneWeights.Add(newBoneWeight);
			}

			return result;
		}

		private static void AddValue(List<Vector2> vectors, int from, int to, float ratioL)
		{
			Vector2 nFrom = vectors[from];
			Vector2 nTo = vectors[to];
			Vector2 nNew = Vector2.Lerp(nFrom, nTo, ratioL);
			vectors.Add(nNew);
		}

		/// <summary>
		/// Close the mesh
		/// </summary>
		public List<PolyMeshData> CapSlice(Material capMaterial)
		{
			Profiler.BeginSample("CapSlice");
			// get all section's vertices and create a meshes from them.
			// Then combine these meshes to single 'sectionMesh' (with one sub-mesh),
			// add this mesh to 'mesh' and add new section material

			var edgeLoops = GetEdgeLoops();
			var outerLoops = new List<BzPolyLoop>(edgeLoops.Count);
			var innerLoops = new List<BzPolyLoop>(edgeLoops.Count);
			foreach (var edgeLoop in edgeLoops)
			{
				var loop = new BzPolyLoop(_meshData, edgeLoop, _plane.normal, _adapter);
				if (!loop.Created)
					continue;

				if (loop.OuterLoop)
					outerLoops.Add(loop);
				else
					innerLoops.Add(loop);
			}

			var capMeshes = new List<PolyMeshData>(outerLoops.Count);
			var inners = innerLoops.ToArray();

			for (int i = 0; i < outerLoops.Count; i++)
			{
				var outer = outerLoops[i];
				var poly = new BzPoly(outer, inners);
				if (!poly.Created)
					continue;

				capMeshes.Add(poly.GetMeshData());
			}

			if (capMeshes.Count != 0)
			{
				var newTriangles = GetNewTriangles(capMeshes, _meshData.SubMeshCount); // TODO: save to the same material if exists
				++_meshData.SubMeshCount;
				_meshData.Triangles.AddRange(newTriangles);

				if (_meshData.MaterialsExists)
				{
					Material[] materials = _meshData.Materials;
					var newMaterials = new Material[materials.Length + 1];
					for (int i = 0; i < materials.Length; ++i)
						newMaterials[i] = materials[i];

					newMaterials[materials.Length] = capMaterial;
					_meshData.Materials = newMaterials;
				}
			}

			Profiler.EndSample();
			return capMeshes;
		}

		/// <summary>
		/// Get vertices that resides on section edge, combined to sequences
		/// </summary>
		public List<LinkedLoop<int>> GetEdgeLoops()
		{
			LinkedList<LinkedLoop<int>> edgeIndexLoops = GetEdgeLoopsByIndex();

			EdgeLoops_JoinBySameValue(edgeIndexLoops);

			// prepare result
			var result = new List<LinkedLoop<int>>(edgeIndexLoops.Count);
			LinkedListNode<LinkedLoop<int>> parrentListsNode = edgeIndexLoops.First;

			while (parrentListsNode != null)
			{
				var indices = parrentListsNode.Value;
				parrentListsNode = parrentListsNode.Next;

				Vector3 firstValue = _meshData.Vertices[indices.first.value];
				Vector3 lastValue = _meshData.Vertices[indices.last.value];
				bool closed = (firstValue - lastValue).sqrMagnitude < MinPointDistanceSqr;
				if (_skipIfNotClosed & closed)
				{
					continue;
				}

				if (closed)
				{
					indices.first.Remove();
				}

				Vector3 prevValue = lastValue;
				var indexNode = indices.first;
				int counter = indices.size;
				while (counter != 0)
				{
					--counter;
					var current = indexNode;
					indexNode = indexNode.next;

					var v = _meshData.Vertices[current.value];

					if (prevValue == v)
					{
						// delete same positions
						current.Remove();
					}
					else
					{
						prevValue = v;
					}
				}

				if (indices.size > 2)
					result.Add(indices);
			}

			return result;
		}

		public void EdgeLoops_JoinBySameValue(LinkedList<LinkedLoop<int>> edgeLoops)
		{
			// join by the most close points
			var parrentListsNode = edgeLoops.First;
			while (parrentListsNode != null)
			{
				var childList = parrentListsNode.Value;
				bool jointFound = false;

				var listFirstValue = _meshData.Vertices[childList.first.value];
				var listLastValue = _meshData.Vertices[childList.last.value];

				var joinCandidate = parrentListsNode.Next;
				while (joinCandidate != null)
				{
					var restFirstValue = _meshData.Vertices[joinCandidate.Value.first.value];
					var restLastValue = _meshData.Vertices[joinCandidate.Value.last.value];

					if ((listLastValue - restFirstValue).sqrMagnitude < MinPointDistanceSqr)
					{
						joinCandidate.Value.first.Remove(); // to avoid duplication of the same index when concatenation
						childList = LinkedLoop.ConcatList(childList, joinCandidate.Value);
						jointFound = true;
						break;
					}
					else if ((listFirstValue - restLastValue).sqrMagnitude < MinPointDistanceSqr)
					{
						childList.first.Remove();
						childList = LinkedLoop.ConcatList(joinCandidate.Value, childList);
						jointFound = true;
						break;
					}

					joinCandidate = joinCandidate.Next;
				}

				if (jointFound)
				{
					edgeLoops.Remove(joinCandidate);
					parrentListsNode.Value = childList;
				}
				else
				{
					parrentListsNode = parrentListsNode.Next;
				}
			}
		}

		public LinkedList<LinkedLoop<int>> GetEdgeLoopsByIndex()
		{
			var lists = new LinkedList<LinkedLoop<int>>();

			// join by common indexes
			var capEdgesLoop = new LinkedLoop<IndexVector>(CapEdges);
			while (capEdgesLoop.size != 0)
			{
				var newList = new LinkedLoop<int>();
				lists.AddLast(newList);

				IndexVector edge = capEdgesLoop.first.value;
				capEdgesLoop.first.Remove();

				newList.AddLast(edge.From);
				newList.AddLast(edge.To);

				var current = capEdgesLoop.first;

				int counter = capEdgesLoop.size;
				while (counter != 0)
				{
					--counter;
					var next = current.next;

					if (TryAttachEdgeToList(current.value, newList))
					{
						current.Remove();
						counter = capEdgesLoop.size;
					}

					current = next;
				}
			}

			return lists;
		}

		private static bool TryAttachEdgeToList(IndexVector segment, LinkedLoop<int> toList)
		{
			var first = toList.first.value;
			if (first == segment.To)
			{
				toList.AddFirst(segment.From);
				return true;
			}

			var last = toList.last.value;
			if (last == segment.From)
			{
				toList.AddLast(segment.To);
				return true;
			}

			return false;
		}

		private List<BzTriangle> GetNewTriangles(List<PolyMeshData> meshes, int newSubMeshIndex)
		{
			int totalTriangleSize = 0;

			for (int m = 0; m < meshes.Count; m++)
			{
				var add = meshes[m];
				int trCount = add.triangles.Length / 3;
				totalTriangleSize += trCount * 2;
			}

			var triangles = new List<BzTriangle>(totalTriangleSize);

			GeneratePolyTriangles(meshes, newSubMeshIndex, triangles, false);
			GeneratePolyTriangles(meshes, newSubMeshIndex, triangles, true);

			return triangles;
		}

		private void GeneratePolyTriangles(List<PolyMeshData> meshes, int newSubMeshIndex, List<BzTriangle> triangles, bool side)
		{
			for (int m = 0; m < meshes.Count; m++)
			{
				int triangleValueShift = _meshData.Vertices.Count;

				var add = meshes[m];

				for (int i = 0; i < add.vertices.Length; i++)
				{
					var vertex = add.vertices[i];
					var normal = add.normals[i];
					var uv = add.uv[i];
					BoneWeight? boneWeight = null;
					Vector4? tangent = null;

					if (add.boneWeights.Length > 0)
						boneWeight = add.boneWeights[i];
					if (add.tangents.Length > 0)
						tangent = add.tangents[i];

					if (side)
					{
						normal = -normal;
						uv = -uv;
					}

					AddVertex(vertex, normal, null, null, uv, null, null, null, tangent, boneWeight);
				}

				var addTriangles = add.triangles;
				int trCount = addTriangles.Length / 3;
				for (int i = 0; i < trCount; ++i)
				{
					int index = i * 3;
					int i1 = addTriangles[index + 0] + triangleValueShift;
					int i2 = addTriangles[index + 1] + triangleValueShift;
					int i3 = addTriangles[index + 2] + triangleValueShift;

					if (side)
					{
						int tmp = i2;
						i2 = i3;
						i3 = tmp;
					}

					var tr = new BzTriangle(
						i1, i2, i3,
						_meshData.WorldVertices[i1], _meshData.WorldVertices[i2], _meshData.WorldVertices[i3],
						newSubMeshIndex, side
						);

					triangles.Add(tr);
				}
			}
		}

		private void AddVertex(Vector3 vertex, Vector3? normal, Color? color, Color32? color32, Vector2? uv, Vector2? uv2, Vector2? uv3, Vector2? uv4, Vector4? tangent, BoneWeight? boneWeight)
		{
			_meshData.Vertices.Add(vertex);

			if (_meshData.NormalsExists)
				_meshData.Normals.Add(normal.GetValueOrDefault());

			if (_meshData.ColorsExists)
				_meshData.Colors.Add(color.GetValueOrDefault());
			if (_meshData.Colors32Exists)
				_meshData.Colors32.Add(color32.GetValueOrDefault());

			if (_meshData.UVExists)
				_meshData.UV.Add(uv.GetValueOrDefault());
			if (_meshData.UV2Exists)
				_meshData.UV2.Add(uv2.GetValueOrDefault());
			if (_meshData.UV3Exists)
				_meshData.UV3.Add(uv3.GetValueOrDefault());
			if (_meshData.UV4Exists)
				_meshData.UV4.Add(uv4.GetValueOrDefault());

			if (_meshData.TangentsExists)
				_meshData.Tangents.Add(tangent.GetValueOrDefault());

			if (_meshData.BoneWeightsExists)
				_meshData.BoneWeights.Add(boneWeight.GetValueOrDefault());

			int index = _meshData.Vertices.Count - 1;
			var worldVertex = _adapter.GetWorldPos(_meshData, index);
			_meshData.WorldVertices.Add(worldVertex);
		}

		private void DivideTriangle(int v, int a, int b, List<BzTriangle> triangles, int subMeshIndex, bool sideOfAB)
		{
			//             v
			//            / \
			//           /   \
			//    new2  /     \  new1
			//      ----.----------
			//        /   .     \
			//       /       .   \
			//    b /___________._\ a

			int new1;
			int new2;
			if (sideOfAB)
			{
				new2 = GetIndexFor(v, b);
				new1 = GetIndexFor(v, a);
			}
			else
			{
				new1 = GetIndexFor(a, v);
				new2 = GetIndexFor(b, v);
			}

			var worldVertices = _meshData.WorldVertices;
			if (new1 == v | new2 == v)
			{
				var tr = new BzTriangle(b, v, a, worldVertices[b], worldVertices[v], worldVertices[a], subMeshIndex, sideOfAB);
				triangles.Add(tr);
			}
			else
			{
				var trV = new BzTriangle(v, new1, new2, worldVertices[v], worldVertices[new2], worldVertices[new1], subMeshIndex, !sideOfAB);
				triangles.Add(trV);

				if (new1 != a)
				{
					var trA = new BzTriangle(a, new2, new1, worldVertices[a], worldVertices[new2], worldVertices[new1], subMeshIndex, sideOfAB);
					triangles.Add(trA);
				}
				if (new2 != b)
				{
					var trB = new BzTriangle(b, new2, a, worldVertices[b], worldVertices[new2], worldVertices[a], subMeshIndex, sideOfAB);
					triangles.Add(trB);
				}
			}

			// save edge to cash
			if (new1 != new2)
			{
				if (sideOfAB)
				{
					CapEdges.Add(new IndexVector(new2, new1));
				}
				else
				{
					CapEdges.Add(new IndexVector(new1, new2));
				}
			}
		}

		/// <summary>
		/// Get ratio from 'v1' to plane
		/// </summary>
		private float CalcGetRatio(Vector3 v1, Vector3 v2, out Vector3 enter)
		{
			var vOldOrigin = v1;
			var vOldDir = v2 - vOldOrigin;

			var ray = new Ray(vOldOrigin, vOldDir);

			float enterD;
			_plane.Raycast(ray, out enterD);

			var r = enterD / vOldDir.magnitude;
			enter = v1 + vOldDir * r;
			return r;
		}
	}

	/// <summary>
	/// Vector from 'From' to 'To'
	/// </summary>
	public struct IndexVector : IEquatable<IndexVector>
	{
		public IndexVector(int from, int to)
		{
			From = from;
			To = to;
		}
		public readonly int From;
		public readonly int To;

		public bool Equals(IndexVector obj)
		{
			return From == obj.From & To == obj.To;
		}

		public override string ToString()
		{
			return "(" + From.ToString() + " -> " + To.ToString() + ")";
		}
	}
}

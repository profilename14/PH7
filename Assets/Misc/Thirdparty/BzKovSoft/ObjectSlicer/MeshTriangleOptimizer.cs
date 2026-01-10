using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Mesh data optimizer
	/// </summary>
	public static class MeshTriangleOptimizer
	{
		private const float MinPointDistanceSqr = 0.00001f;
		private const float MinPointAngle = 0.000001f;

		public static void OptimizeEdgeTriangles(BzMeshDataEditor meshDataEditor, BzMeshData meshData, List<BzTriangle> bzTriangles)
		{
			LinkedList<LinkedLoop<int>> edgeLoopsByIndex = meshDataEditor.GetEdgeLoopsByIndex();
			bool[] trToDelete = new bool[bzTriangles.Count];
			var indicesToDelete = new List<int>(64);

			var edgeLoopsNode = edgeLoopsByIndex.First;
			while (edgeLoopsNode != null)
			{
				var edgeLoop = edgeLoopsNode.Value;
				edgeLoopsNode = edgeLoopsNode.Next;

				var edge = edgeLoop.first;
				int counter = edgeLoop.size;
				while (counter > 0 & edgeLoop.size >= 3)
				{
					var edgeItem1 = edge;
					var edgeItem2 = edgeItem1.next;
					var edgeItem3 = edgeItem2.next;

					int i1 = edgeItem1.value;
					int i2 = edgeItem2.value;
					int i3 = edgeItem3.value;

					var v1 = meshData.Vertices[i1];
					var v2 = meshData.Vertices[i2];
					var v3 = meshData.Vertices[i3];

					var dir1 = v2 - v1;
					var dir2 = v3 - v2;
					float angle = Vector3.Angle(BzSlicerHelper.Normalize(dir1), BzSlicerHelper.Normalize(dir2));

					bool zeroAngle = angle < MinPointAngle;
					bool zeroDistance = dir1.sqrMagnitude < MinPointDistanceSqr | dir2.sqrMagnitude < MinPointDistanceSqr;

					bool success = false;
					if (zeroAngle | zeroDistance)
					{
						success = RemoveRedundantIndex(i3, i2, i1, bzTriangles, trToDelete);
					}

					if (success)
					{
						edgeItem2.Remove();
						indicesToDelete.Add(i2);
					}
					else
					{
						edge = edge.next;
						--counter;
					}
				}
			}

			// repair caps
			foreach (var indexToDelete in indicesToDelete)
			{
				var begins = meshDataEditor.CapEdges.Where(_ => _.To == indexToDelete).ToArray();
				var ends = meshDataEditor.CapEdges.Where(_ => _.From == indexToDelete).ToArray();
				if (ends.Length != 1 | begins.Length != 1)
				{
					continue;
				}

				var begin = begins[0];
				var end = ends[0];

				meshDataEditor.CapEdges.Remove(begin);
				meshDataEditor.CapEdges.Remove(end);
				meshDataEditor.CapEdges.Add(new IndexVector(begin.From, end.To));
			}

			// remove empty
			int count = 0;
			for (int i = 0; i < bzTriangles.Count; i++)
			{
				if (!trToDelete[i])
				{
					var value = bzTriangles[i];
					bzTriangles[count] = value;
					++count;
				}
			}

			bzTriangles.RemoveRange(count, bzTriangles.Count - count);
		}

		private static bool RemoveRedundantIndex(int indexLeft, int indexMiddle, int indexRight, List<BzTriangle> bzTriangles, bool[] trToDelete)
		{
			// make redundants empty
			int trIndexNegRight = -1;
			int trIndexNegLeft = -1;
			int trIndexPosRight = -1;
			int trIndexPosLeft = -1;
			int trInnerIndexNeg = -1;
			int trInnerIndexPos = -1;
			for (int i = 0; i < bzTriangles.Count; i++)
			{
				var tr = bzTriangles[i];
				if (trToDelete[i])
				{
					continue;
				}

				if (tr.i1 == indexMiddle | tr.i2 == indexMiddle | tr.i3 == indexMiddle)
				{
					if (tr.i1 == indexRight | tr.i2 == indexRight | tr.i3 == indexRight)
					{
						if (tr.side)
						{
							if (trIndexPosRight != -1) return false;
							trIndexPosRight = i;
						}
						else
						{
							if (trIndexNegRight != -1) return false;
							trIndexNegRight = i;
						}
					}
					else if (tr.i1 == indexLeft | tr.i2 == indexLeft | tr.i3 == indexLeft)
					{
						if (tr.side)
						{
							if (trIndexPosLeft != -1) return false;
							trIndexPosLeft = i;
						}
						else
						{
							if (trIndexNegLeft != -1) return false;
							trIndexNegLeft = i;
						}
					}
					else
					{
						if (tr.side)
						{
							if (trInnerIndexPos != -1) return false;
							trInnerIndexPos = i;
						}
						else
						{
							if (trInnerIndexNeg != -1) return false;
							trInnerIndexNeg = i;
						}
					}
				}
			}

			if (trIndexNegLeft == -1 | trIndexNegRight == -1 |
				trIndexPosRight == -1 | trIndexPosLeft == -1)
			{
				return false;
			}

			RemoveRedundantIndex(indexLeft, indexMiddle, indexRight, bzTriangles, trToDelete, trIndexNegRight, trIndexNegLeft, trInnerIndexNeg);
			RemoveRedundantIndex(indexRight, indexMiddle, indexLeft, bzTriangles, trToDelete, trIndexPosLeft, trIndexPosRight, trInnerIndexPos);

			return true;
		}

		private static void RemoveRedundantIndex(int indexLeft, int indexMiddle, int indexRight, List<BzTriangle> bzTriangles, bool[] trToDelete, int trIndexRight, int trIndexLeft, int trInnerIndex)
		{
			var trRight = bzTriangles[trIndexRight];
			var trLeft = bzTriangles[trIndexLeft];
			GetIndexesOrdered(trRight, indexMiddle, out _, out int rootIndexRight, out Vector3 vectorRight, out Vector3 rootVectorRight);
			GetIndexesOrdered(trLeft, indexMiddle, out int rootIndexLeft, out _, out Vector3 rootVectorLeft, out Vector3 vectorLeft);

			if (rootIndexLeft == rootIndexRight)
			{
				bzTriangles[trIndexLeft] = new BzTriangle(
					rootIndexLeft, indexLeft, indexRight,
					rootVectorLeft, vectorLeft, vectorRight,
					trLeft.subMeshIndex, trLeft.side
					);
				trToDelete[trIndexRight] = true;
			}
			else
			{
				bzTriangles[trIndexLeft] = new BzTriangle(
					rootIndexLeft, indexLeft, indexRight,
					rootVectorLeft, vectorLeft, vectorRight,
					trLeft.subMeshIndex, trLeft.side
					);
				bzTriangles[trIndexRight] = new BzTriangle(
					rootIndexLeft, indexRight, rootIndexRight,
					rootVectorLeft, vectorRight, rootVectorRight,
					trRight.subMeshIndex, trRight.side
					);

				if (trInnerIndex != -1)
				{
					trToDelete[trInnerIndex] = true;
				}
			}
		}

		private static bool GetIndexesOrdered(BzTriangle tr, int i1, out int i2, out int i3, out Vector3 v2, out Vector3 v3)
		{
			if (tr.i1 == i1)
			{
				i2 = tr.i2;
				i3 = tr.i3;
				v2 = tr.v2;
				v3 = tr.v3;
				return true;
			}
			
			if (tr.i2 == i1)
			{
				i2 = tr.i3;
				i3 = tr.i1;
				v2 = tr.v3;
				v3 = tr.v1;
				return true;
			}
			
			if (tr.i3 == i1)
			{
				i2 = tr.i1;
				i3 = tr.i2;
				v2 = tr.v1;
				v3 = tr.v2;
				return true;
			}

			i2 = -1;
			i3 = -2;
			v2 = Vector3.zero;
			v3 = Vector3.zero;
			return false;
		}
	}
}
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.Tests
{
	public class MeshTriangleOptimizerTests
	{
		[Test]
		public void Optimize()
		{
			//                 /6\
			//              /  / \  \
			//            /   /   \   \
			//          /    /     \    \
			//        /     /       \     \
			//      /      /         \      \
			//    /       /           \       \
			//  1         4           5         2
			//   |--------*-----------|--------|
			//   |        | \         \        |
			//   |   t0   /  \   t2    |  t3   |
			//   |       |     \       \       |
			//   |      /       \       \      |
			//   |      /         \      |     |
			//   |     |           \     \     |
			//   |    /              \    \    |
			//   |    /               \    |   |
			//   |   |                  \  \   |
			//   |  /   t1               \  \  |
			//   |  /                      \ | |
			//   | |                        \\ |
			//   |/___________________________\|
			//  0                               3

			//Arrange
			var vertices = new Vector3[]
			{
				new Vector3(-10,-10, 0),
				new Vector3(-10, 10, 0),
				new Vector3( 10, 10, 0),
				new Vector3( 10,-10, 0),

				new Vector3( -5, 10, 0),  // 4
				new Vector3(  5, 10, 0),  // 5
				new Vector3(  0, 20, 0),  // 6 - up outside
			};

			List<BzTriangle> trianglesSliced = new List<BzTriangle>
			{
				GetTriangle(vertices, 1, 4, 0, false),
				GetTriangle(vertices, 4, 3, 0, false),
				GetTriangle(vertices, 4, 5, 3, false),
				GetTriangle(vertices, 5, 2, 3, false),

				GetTriangle(vertices, 2, 5, 6, true),
				GetTriangle(vertices, 5, 4, 6, true),
				GetTriangle(vertices, 4, 1, 6, true),
			};

			var mesh = new Mesh();
			mesh.vertices = vertices;

			var meshData = new BzMeshData(mesh, null);
			var adapter = new BzManualMeshAdapter();
			BzMeshDataEditor editor = new BzMeshDataEditor(meshData, new Plane(), adapter, false);

			editor.CapEdges.Add(new IndexVector(2, 5));
			editor.CapEdges.Add(new IndexVector(5, 4));
			editor.CapEdges.Add(new IndexVector(4, 1));

			//Act
			MeshTriangleOptimizer.OptimizeEdgeTriangles(editor, meshData, trianglesSliced);

			//Assert
			Assert.AreEqual(3, trianglesSliced.Count);

			Assert.That(IsTrExists(trianglesSliced, GetTriangle(vertices, 6, 2, 1, true)));

			IsTrExists(trianglesSliced, GetTriangle(vertices, 1, 2, 0, false));
			bool case1 =
				IsTrExists(trianglesSliced, GetTriangle(vertices, 1, 2, 3, false)) &&
				IsTrExists(trianglesSliced, GetTriangle(vertices, 1, 3, 0, false));
			bool case2 =
				IsTrExists(trianglesSliced, GetTriangle(vertices, 1, 2, 0, false)) &&
				IsTrExists(trianglesSliced, GetTriangle(vertices, 2, 3, 0, false));
			Assert.That(case1 ^ case2);
		}

		[Test]
		[Ignore("Several inners are not supported")]
		[TestCase(false)]
		[TestCase(true)]
		public void DiffDirections(bool diffDir)
		{
			//  1         4         2
			//   |--------|--------|
			//   |        |\       |
			//   |   t0   | |  t3  |
			//   |       || \      |
			//   |      / |  |     |
			//   |      / |  \     |
			//   |     |  |   |    |
			//   |    /   |   \    |
			//   |    /   |    |   |
			//   |   |    |    \   |
			//   |  /   t1| t2  |  |
			//   |  /     |     \  |
			//   | |      |      | |
			//   |/_______|_     | |
			//  0         5 `"--..\|
			//                      3

			//Arrange
			var vertices = new Vector3[]
			{
				new Vector3(-10,-10, 0),
				new Vector3(-10, 10, 0),
				new Vector3( 10, 10, 0),
				new Vector3( 10, diffDir ? -15 : -10, 0),

				new Vector3(  0, 10, 0),  // 4
				new Vector3(  0,-10, 0),  // 5
			};

			List<BzTriangle> trianglesSliced = new List<BzTriangle>
			{
				GetTriangle(vertices, 1, 4, 0, false),
				GetTriangle(vertices, 4, 5, 0, false),
				GetTriangle(vertices, 4, 3, 5, false),
				GetTriangle(vertices, 4, 2, 3, false),
			};

			var mesh = new Mesh();
			mesh.vertices = vertices;

			var meshData = new BzMeshData(mesh, null);
			var adapter = new BzManualMeshAdapter();
			BzMeshDataEditor editor = new BzMeshDataEditor(meshData, new Plane(), adapter, false);

			editor.CapEdges.Add(new IndexVector(2, 4));
			editor.CapEdges.Add(new IndexVector(4, 1));

			//Act
			MeshTriangleOptimizer.OptimizeEdgeTriangles(editor, meshData, trianglesSliced);

			//Assert
			if (diffDir)
			{
				Assert.That(trianglesSliced.Count >= 3);
			}
			else
			{
				Assert.AreEqual(2, trianglesSliced.Count);
			}
		}

		[Test]
		[Ignore("Several inners are not supported")]
		[TestCase(false)]
		[TestCase(true)]
		public void BigAngle(bool mirror)
		{
			//   0     1                           2
			// --|-----|.-------------------------|--
			//   \      \ *._                    /
			//     \      \  *._                /
			//       \     \    *._            /
			//         \     \     *._        /
			//           \    \       *._    /
			//             \    \        *_ /
			//               \   \         |  3
			//                 \   \       |
			//                   \  \      |
			//                     \  \    |
			//                       \ \   |
			//                         \ \ |
			//                           \\|
			//                        -----*-----
			//                             4

			//Arrange
			float m = mirror ? -1f : 1f;
			var vertices = new Vector3[]
			{
				new Vector3(m * -10, 0, 0),
				new Vector3(m *  -5, 0, 0),
				new Vector3(m *  10, 0, 0),

				new Vector3(  0,-1, 0),
				new Vector3(  0,-5, 0),
			};

			List<BzTriangle> trianglesSliced = new List<BzTriangle>
			{
				GetOrder(GetTriangle(vertices, 0, 1, 4, false), mirror),
				GetOrder(GetTriangle(vertices, 1, 3, 4, false), mirror),
				GetOrder(GetTriangle(vertices, 1, 2, 3, false), mirror),
			};

			var mesh = new Mesh();
			mesh.vertices = vertices;

			var meshData = new BzMeshData(mesh, null);
			var adapter = new BzManualMeshAdapter();
			BzMeshDataEditor editor = new BzMeshDataEditor(meshData, new Plane(), adapter, false);
			if (mirror)
			{
				editor.CapEdges.Add(new IndexVector(0, 1));
				editor.CapEdges.Add(new IndexVector(1, 2));
			}
			else
			{
				editor.CapEdges.Add(new IndexVector(2, 1));
				editor.CapEdges.Add(new IndexVector(1, 0));
			}

			//Act
			MeshTriangleOptimizer.OptimizeEdgeTriangles(editor, meshData, trianglesSliced);

			//Assert
			Assert.AreEqual(2, trianglesSliced.Count);
			var tr1 = trianglesSliced[0];
			var tr2 = trianglesSliced[1];

			bool case1 =
				TrAreEqual(tr1, GetOrder(GetTriangle(vertices, 0, 2, 3, false), mirror)) &&
				TrAreEqual(tr2, GetOrder(GetTriangle(vertices, 0, 3, 4, false), mirror));
			bool case2 =
				TrAreEqual(tr1, GetOrder(GetTriangle(vertices, 0, 3, 4, false), mirror)) &&
				TrAreEqual(tr2, GetOrder(GetTriangle(vertices, 0, 2, 3, false), mirror));

			Assert.That(case1 ^ case2);
		}

		private static BzTriangle GetTriangle(Vector3[] vertices, int i1, int i2, int i3, bool side)
		{
			var v1 = vertices[i1];
			var v2 = vertices[i2];
			var v3 = vertices[i3];
			var tr = new BzTriangle(i1, i2, i3, v1, v2, v3, 0, side);
			return tr;
		}

		private static BzTriangle GetOrder(BzTriangle tr, bool mirror)
		{
			if (!mirror)
			{
				return tr;
			}

			return new BzTriangle(tr.i2, tr.i1, tr.i3, tr.v2, tr.v1, tr.v3, tr.subMeshIndex, tr.side);
		}

		private bool IsTrExists(List<BzTriangle> triangles, BzTriangle triangle)
		{
			return triangles.Any(_ => TrAreEqual(_, triangle));
		}

		private bool TrAreEqual(BzTriangle trA, BzTriangle trB)
		{
			if (trA.subMeshIndex != trB.subMeshIndex ||
				trA.side != trB.side)
			{
				return false;
			}

			trA = GetOrdered(trA);
			trB = GetOrdered(trB);
			return
				trA.i1 == trB.i1 &
				trA.i2 == trB.i2 &
				trA.i3 == trB.i3;
		}

		private BzTriangle GetOrdered(BzTriangle tr)
		{
			if (tr.i1 <= tr.i2 & tr.i1 <= tr.i3)
			{
				return tr;
			}
			if (tr.i2 <= tr.i1 & tr.i2 <= tr.i3)
			{
				return new BzTriangle(tr.i2, tr.i3, tr.i1, tr.v2, tr.v3, tr.v1, tr.subMeshIndex, tr.side);
			}
			if (tr.i3 <= tr.i1 & tr.i3 <= tr.i2)
			{
				return new BzTriangle(tr.i3, tr.i1, tr.i2, tr.v3, tr.v1, tr.v2, tr.subMeshIndex, tr.side);
			}

			throw new InvalidOperationException();
		}
	}
}

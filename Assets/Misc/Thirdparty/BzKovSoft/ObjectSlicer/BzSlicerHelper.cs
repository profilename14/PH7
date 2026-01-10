using System;
using System.Collections.Generic;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	public static class BzSlicerHelper
	{
		public static T GetSameComponentForDuplicate<T>(T c, GameObject original, GameObject duplicate)
			where T : Component
		{
			// remember hierarchy
			Stack<int> path = new Stack<int>();

			var g = c.gameObject;
			while (!object.ReferenceEquals(g, original))
			{
				path.Push(g.transform.GetSiblingIndex());
				g = g.transform.parent.gameObject;
			}

			// repeat hierarchy on duplicated object
			GameObject sameGO = duplicate;
			while (path.Count != 0)
			{
				sameGO = sameGO.transform.GetChild(path.Pop()).gameObject;
			}

			// get component index
			var cc = c.gameObject.GetComponents<T>();
			int componentIndex = -1;
			for (int i = 0; i < cc.Length; i++)
			{
				if (object.ReferenceEquals(c, cc[i]))
				{
					componentIndex = i;
					break;
				}
			}

			// return component with the same index on same gameObject
			return sameGO.GetComponents<T>()[componentIndex];
		}

		public static Vector3 Normalize(Vector3 v)
		{
			// I do not know why, but standard v.Normalized do not work with very small values

			float magnitudeSqr = v.x * v.x + v.y * v.y + v.z * v.z;
			float magnitude = Mathf.Sqrt(magnitudeSqr);
			float f = 1f / magnitude;
			return new Vector3(v.x * f, v.y * f, v.z * f);
		}

		public static void GroupTrianglesToMeshes(BzMeshData meshData, bool objectGrouping)
		{
			var meshes = new List<ShortMeshData>();
			bool intersected = false;
			foreach (var tr in meshData.Triangles)
			{
				intersected = false;
				ShortMeshData asd = null;
				foreach (var mesh in meshes)
				{
					if (mesh.side != tr.side)
					{
						continue;
					}

					if (objectGrouping)
					{
						if (mesh.indices.Contains(tr.i1) ||
							mesh.indices.Contains(tr.i2) ||
							mesh.indices.Contains(tr.i3) ||
							mesh.vertices.Contains(tr.v1) ||
							mesh.vertices.Contains(tr.v2) ||
							mesh.vertices.Contains(tr.v3))
						{
							intersected = true;
						}
					}
					else
					{
						intersected = true;
					}


					if (intersected)
					{
						asd = mesh;
						break;
					}
				}

				if (!intersected)
				{
					asd = new ShortMeshData();
					asd.side = tr.side;
					asd.triangles = new List<BzTriangle>(meshData.Triangles.Count / (meshes.Count + 1));
					asd.indices = new HashSet<int>();
					asd.vertices = new HashSet<Vector3>();
					meshes.Add(asd);
				}

				asd.triangles.Add(tr);
				asd.indices.Add(tr.i1);
				asd.indices.Add(tr.i2);
				asd.indices.Add(tr.i3);
				asd.vertices.Add(tr.v1);
				asd.vertices.Add(tr.v2);
				asd.vertices.Add(tr.v3);
			}

			do
			{
				meshes.Sort((a, b) => -b.triangles.Count.CompareTo(a.triangles.Count));
				intersected = FindIntersection(meshes, out var firstMesh, out var secondMesh);

				if (intersected)
				{
					if (firstMesh.triangles.Count > secondMesh.triangles.Count)
					{
						firstMesh.triangles.AddRange(secondMesh.triangles);
						firstMesh.indices.UnionWith(secondMesh.indices);
						firstMesh.vertices.UnionWith(secondMesh.vertices);
						meshes.Remove(secondMesh);
					}
					else
					{
						secondMesh.triangles.AddRange(firstMesh.triangles);
						secondMesh.indices.UnionWith(firstMesh.indices);
						secondMesh.vertices.UnionWith(firstMesh.vertices);
						meshes.Remove(firstMesh);
					}
				}
			}
			while (intersected);


			var result = new List<BzMeshItemData>(meshes.Count);
			for (int i = 0; i < meshes.Count; i++)
			{
				var mesh = meshes[i];
				var resultItem = new BzMeshItemData();
				resultItem.side = mesh.side;
				resultItem.triangles = mesh.triangles;
				result.Add(resultItem);
			}

			meshData.Meshes = result;
		}

		public static float VolumeOfMesh(List<BzTriangle> triangles, out Vector3 centerOfMass, out Vector3 centerOfPoints)
		{
			centerOfMass = Vector3.zero;
			centerOfPoints = Vector3.zero;

			float volTotal = 0;

			for (int i = 0; i < triangles.Count; ++i)
			{
				var tr = triangles[i];

				float vol = SignedVolumeOfTriangle(tr.v1, tr.v2, tr.v3);
				volTotal += vol;

				Vector3 trCenter = GetTetrahedronCenter(tr.v1, tr.v2, tr.v3);
				centerOfMass += trCenter * vol;
				centerOfPoints += (tr.v1 + tr.v2 + tr.v3) / 3f;
			}

			centerOfMass /= volTotal;
			centerOfPoints /= triangles.Count;

			return Math.Abs(volTotal);
		}

		private static bool FindIntersectionByPoints(ShortMeshData mesh1, ShortMeshData mesh2)
		{
			foreach (var tr1 in mesh1.triangles)
			{
				if (mesh2.indices.Contains(tr1.i1) ||
					mesh2.indices.Contains(tr1.i2) ||
					mesh2.indices.Contains(tr1.i3) ||
					mesh2.vertices.Contains(tr1.v1) ||
					mesh2.vertices.Contains(tr1.v2) ||
					mesh2.vertices.Contains(tr1.v3))
				{
					return true;
				}
			}

			return false;
		}

		private static bool FindIntersection(List<ShortMeshData> meshes, out ShortMeshData firstMesh, out ShortMeshData secondMesh)
		{
			for (int meshFirstIndex = 0; meshFirstIndex < meshes.Count; meshFirstIndex++)
			{
				var meshFirst = meshes[meshFirstIndex];
				for (int meshSecondIndex = meshFirstIndex + 1; meshSecondIndex < meshes.Count; meshSecondIndex++)
				{
					var meshSecond = meshes[meshSecondIndex];

					if (meshFirst.side != meshSecond.side)
					{
						continue;
					}

					if (FindIntersectionByPoints(meshFirst, meshSecond))
					{
						firstMesh = meshFirst;
						secondMesh = meshSecond;
						return true;
					}
				}
			}

			firstMesh = null;
			secondMesh = null;
			return false;
		}

		private static Vector3 GetTetrahedronCenter(Vector3 v1, Vector3 v2, Vector3 v3)
		{
			return (v1 + v2 + v3) / 4f;
		}

		private static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			var m = new Matrix4x4()
			{
				m00 = p1.x,
				m01 = p2.x,
				m02 = p3.x,
				m03 = 0f,
				m10 = p1.y,
				m11 = p2.y,
				m12 = p3.y,
				m13 = 0f,
				m20 = p1.z,
				m21 = p2.z,
				m22 = p3.z,
				m23 = 0f,
				m30 = 1f,
				m31 = 1f,
				m32 = 1f,
				m33 = 1f
			};
			return (1f / 6f) * m.determinant;
		}

		private class ShortMeshData
		{
			public bool side;
			public List<BzTriangle> triangles;
			public HashSet<int> indices;
			public HashSet<Vector3> vertices;
		}
	}
}

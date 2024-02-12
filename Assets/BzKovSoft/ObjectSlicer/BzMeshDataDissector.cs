using System;
using System.Collections.Generic;
using BzKovSoft.ObjectSlicer.Polygon;
using UnityEngine;
using UnityEngine.Profiling;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Mesh separator
	/// </summary>
	public class BzMeshDataDissector
	{
		private const float MinWidth = 0.001f;
		private readonly IBzSliceAdapter _adapter;
		private readonly bool _objectGrouping;
		private readonly Plane _plane;
		private readonly BzMeshData _meshData;
		private readonly int[][] _subMeshes;

		private Material _defaultSliceMaterial;
		private bool _sliced = false;

		/// <summary>
		/// Default material that will be applied on caps serface
		/// </summary>
		public Material DefaultSliceMaterial { get => _defaultSliceMaterial; set => _defaultSliceMaterial = value; }
		public IBzSliceAdapter Adapter => _adapter;
		public BzMeshData MeshData => _meshData;
		public SliceConfigurationDto Configuration { get; private set; }
		public List<PolyMeshData> CapResult { get; private set; }

		public BzMeshDataDissector(Mesh mesh, Plane plane, Material[] materials, IBzSliceAdapter adapter, SliceConfigurationDto configuration, bool objectGrouping)
		{
			_adapter = adapter;
			_plane = plane;
			Configuration = configuration;
			_objectGrouping = objectGrouping;
			_meshData = new BzMeshData(mesh, materials);

			_subMeshes = new int[mesh.subMeshCount][];
			for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; ++subMeshIndex)
			{
				_subMeshes[subMeshIndex] = mesh.GetTriangles(subMeshIndex);
			}
		}

		/// <summary>
		/// Execute separation
		/// </summary>
		public SliceResult Slice()
		{
			if (_sliced)
				throw new InvalidOperationException("Object already sliced");

			_sliced = true;

			switch (Configuration.SliceType)
			{
				case SliceType.Slice:
					return SliceMesh(Configuration.SliceMaterial ?? _defaultSliceMaterial);

				case SliceType.KeepOne:
					return _plane.GetSide(_adapter.GetObjectCenterInWorldSpace()) ?
						SliceResult.Pos : SliceResult.Neg;

				case SliceType.Duplicate:
					return SliceResult.Duplicate;

				default: throw new NotSupportedException();
			}
		}

		private SliceResult SliceMesh(Material sectionViewMaterial)
		{
			Profiler.BeginSample("SliceMesh");

			var worldVectors = new List<Vector3>(_meshData.Vertices.Count);
			for (int i = 0; i < _meshData.Vertices.Count; ++i)
			{
				Vector3 v = _adapter.GetWorldPos(_meshData, i);
				worldVectors.Add(v);
			}
			_meshData.WorldVertices = worldVectors;

			bool skipIfNotClosed = Configuration.SkipIfNotClosed;

			BzMeshDataEditor meshEditor = new BzMeshDataEditor(_meshData, _plane, _adapter, skipIfNotClosed);

			for (int subMeshIndex = 0; subMeshIndex < _subMeshes.Length; ++subMeshIndex)
			{
				int[] newTriangles = _subMeshes[subMeshIndex];

				int trCount = newTriangles.Length / 3;
				var triangles = new List<BzTriangle>(Mathf.RoundToInt(trCount * 1.2f));

				for (int i = 0; i < trCount; ++i)
				{
					int trIndex = i * 3;
					int i1 = newTriangles[trIndex + 0];
					int i2 = newTriangles[trIndex + 1];
					int i3 = newTriangles[trIndex + 2];
					Vector3 v1 = _meshData.WorldVertices[i1];
					Vector3 v2 = _meshData.WorldVertices[i2];
					Vector3 v3 = _meshData.WorldVertices[i3];
					bool side1 = _plane.GetSide(v1);
					bool side2 = _plane.GetSide(v2);
					bool side3 = _plane.GetSide(v3);

					bool posSide = side1 | side2 | side3;
					bool negSide = !side1 | !side2 | !side3;

					if (negSide & posSide)
 					{
						meshEditor.DivideByPlane(
							i1, i2, i3,
							triangles,
							subMeshIndex,
							side1, side2, side3);
					}
					else if (negSide)
					{
						var bzTriangle = new BzTriangle(i1, i2, i3, v1, v2, v3, subMeshIndex, false);
						triangles.Add(bzTriangle);
					}
					else if (posSide)
					{
						var bzTriangle = new BzTriangle(i1, i2, i3, v1, v2, v3, subMeshIndex, true);
						triangles.Add(bzTriangle);
					}
					else
						throw new InvalidOperationException();
				}

				MeshTriangleOptimizer.OptimizeEdgeTriangles(meshEditor, _meshData, triangles);
				_meshData.Triangles.AddRange(triangles);
			}

			if (Configuration.CreateCap)
			{
				CapResult = meshEditor.CapSlice(sectionViewMaterial);
			}

			Profiler.BeginSample("GroupTrianglesToMeshes");
			BzSlicerHelper.GroupTrianglesToMeshes(_meshData, _objectGrouping);
			Profiler.EndSample();

			Profiler.EndSample();

			if (!CheckMinWidth(_meshData, false))
			{
				return SliceResult.Pos;
			}

			if (!CheckMinWidth(_meshData, true))
			{
				return SliceResult.Neg;
			}

			return SliceResult.Sliced;
		}

		private bool CheckMinWidth(BzMeshData meshData, bool side)
		{
			foreach (var mesh in meshData.Meshes)
			{
				foreach (var triangle in mesh.triangles)
				{
					if (triangle.side != side)
						continue;

					if (Math.Abs(_plane.GetDistanceToPoint(triangle.v1)) > MinWidth)
						return true;
					if (Math.Abs(_plane.GetDistanceToPoint(triangle.v2)) > MinWidth)
						return true;
					if (Math.Abs(_plane.GetDistanceToPoint(triangle.v3)) > MinWidth)
						return true;
				}
			}

			return false;
		}
	}
}

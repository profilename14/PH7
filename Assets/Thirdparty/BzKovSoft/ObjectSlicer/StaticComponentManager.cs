using System;
using System.Linq;
using BzKovSoft.ObjectSlicer.MeshGenerator;
using UnityEngine;
using UnityEngine.Profiling;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Manages the components of an object with a static mesh
	/// </summary>
	public class StaticComponentManager : IComponentManager
	{
		protected readonly GameObject _originalObject;
		protected readonly Plane _plane;
		public readonly ColliderSliceResult[] _colliderResults;

		public bool Success { get { return _colliderResults != null; } }

		/// <summary>
		/// Creates a Static Component Manager.
		/// </summary>
		/// <param name="go">The game object being sliced</param>
		/// <param name="plane">The plane by which the slice will be performed</param>
		/// <param name="colliders">The colliders on the game object being sliced</param>
		public StaticComponentManager(GameObject go, Plane plane, Collider[] colliders)
		{
			_originalObject = go;
			_plane = plane;

			_colliderResults = SliceColliders(plane, colliders);
		}

		public void OnSlicedWorkerThread(OneObjectItem[] resultObjects)
		{
			for (int i = 0; i < _colliderResults.Length; i++)
			{
				var colliderResult = _colliderResults[i];

				colliderResult.SliceResult = colliderResult.meshDissector.Slice();

				if (colliderResult.SliceResult == SliceResult.Sliced)
				{
					var closestObjItemNeg = GetClosestObjectItem(colliderResult, resultObjects, false);
					var closestObjItemPos = GetClosestObjectItem(colliderResult, resultObjects, true);

					colliderResult.groupNumberNeg = closestObjItemNeg.groupNumber;
					colliderResult.groupNumberPos = closestObjItemPos.groupNumber;
				}
				else if (colliderResult.SliceResult == SliceResult.Neg)
				{
					var closestObjItem = GetClosestObjectItem(colliderResult, resultObjects, false);
					colliderResult.groupNumberNeg = closestObjItem.groupNumber;
					colliderResult.groupNumberPos = closestObjItem.groupNumber;
				}
				else if (colliderResult.SliceResult == SliceResult.Pos)
				{
					var closestObjItem = GetClosestObjectItem(colliderResult, resultObjects, true);
					colliderResult.groupNumberNeg = closestObjItem.groupNumber;
					colliderResult.groupNumberPos = closestObjItem.groupNumber;
				}
				else
				{
					throw new InvalidOperationException();
				}
			}

			foreach (var resultObjectA in resultObjects)
			{
				bool colliderExistsA = _colliderResults.Any(_ => _.groupNumberNeg == resultObjectA.groupNumber | _.groupNumberPos == resultObjectA.groupNumber);
				if (colliderExistsA)
				{
					continue;
				}

				var centerA = Vector3.zero;
				float totalValumeA = resultObjectA.meshes.Where(_ => _.SliceType == SliceType.Slice).Sum(_ => _.Volume);
				foreach (var mesh in resultObjectA.meshes)
				{
					if (mesh.SliceType == SliceType.Slice)
					{
						centerA += mesh.CenterOfMass / totalValumeA * mesh.Volume;
					}
				}

				float distance = float.MaxValue;

				foreach (var resultObjectB in resultObjects)
				{
					if (resultObjectA.side != resultObjectB.side | resultObjectA.groupNumber == resultObjectB.groupNumber)
					{
						continue;
					}

					bool colliderExistsB = _colliderResults.Any(_ => _.groupNumberNeg == resultObjectB.groupNumber | _.groupNumberPos == resultObjectB.groupNumber);
					if (!colliderExistsB)
					{
						continue;
					}

					float totalValumeB = resultObjectB.meshes.Sum(_ => _.Volume);
					foreach (var mesh in resultObjectB.meshes)
					{
						var centerB = mesh.CenterOfMass;
						float newDistance = (centerA - centerB).sqrMagnitude;
						if (newDistance < distance)
						{
							distance = newDistance;
							resultObjectA.groupNumber = resultObjectB.groupNumber;
						}
					}

				}
			}
		}

		public void OnSlicedMainThread(OneObjectItem[] resultObjects)
		{
			Profiler.BeginSample("RepairColliders");

			for (int i = 0; i < _colliderResults.Length; i++)
			{
				var colliderResult = _colliderResults[i];

				var colliders = new Collider[resultObjects.Length];
				for (int j = 0; j < colliders.Length; j++)
				{
					colliders[j] = BzSlicerHelper.GetSameComponentForDuplicate(colliderResult.OriginalCollider, _originalObject, resultObjects[j].newObject);
				}

				if (colliderResult.SliceResult == SliceResult.Sliced)
				{
					var colliderMeshNeg = colliderResult.meshDissector.MeshData.Meshes.Where(_ => _.side == false).Single();
					var colliderMeshPos = colliderResult.meshDissector.MeshData.Meshes.Where(_ => _.side == true).Single();
					BzGeneratedMesh resultMeshNeg = colliderResult.meshDissector.MeshData.GenerateMeshes(colliderMeshNeg);
					BzGeneratedMesh resultMeshPos = colliderResult.meshDissector.MeshData.GenerateMeshes(colliderMeshPos);

					var slicedColliderNeg = new MeshColliderConf(resultMeshNeg.mesh, colliderResult.OriginalCollider.material);
					var slicedColliderPos = new MeshColliderConf(resultMeshPos.mesh, colliderResult.OriginalCollider.material);

					for (int j = 0; j < resultObjects.Length; j++)
					{
						var collider = colliders[j];
						var resultObject = resultObjects[j];
						var colliderGameObject = collider.gameObject;
						UnityEngine.Object.Destroy(collider);
						if (resultObject.groupNumber == colliderResult.groupNumberNeg)
						{
							AddCollider(slicedColliderNeg, colliderGameObject);
						}
						else if (resultObject.groupNumber == colliderResult.groupNumberPos)
						{
							AddCollider(slicedColliderPos, colliderGameObject);
						}
					}
				}
				else if (colliderResult.SliceResult == SliceResult.Neg)
				{
					for (int j = 0; j < resultObjects.Length; j++)
					{
						if (resultObjects[j].groupNumber != colliderResult.groupNumberNeg)
						{
							UnityEngine.Object.Destroy(colliders[j]);
						}
					}
				}
				else if (colliderResult.SliceResult == SliceResult.Pos)
				{
					for (int j = 0; j < resultObjects.Length; j++)
					{
						if (resultObjects[j].groupNumber != colliderResult.groupNumberPos)
						{
							UnityEngine.Object.Destroy(colliders[j]);
						}
					}
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
			Profiler.EndSample();
		}

		private static OneObjectItem GetClosestObjectItem(ColliderSliceResult colliderResult, OneObjectItem[] resultObjects, bool side)
		{
			float minDist = float.MaxValue;
			OneObjectItem closestObjectItem = null;

			foreach (var resultObject in resultObjects)
			{
				if (resultObject.side != side)
				{
					continue;
				}

				foreach (var mesh in resultObject.meshes)
				{
					float magn = (colliderResult.center - mesh.CenterOfPoints).sqrMagnitude;
					if (magn < minDist)
					{
						minDist = magn;
						closestObjectItem = resultObject;
					}

					foreach (var vertex in colliderResult.meshDissector.MeshData.WorldVertices)
					{
						magn = (vertex - mesh.CenterOfPoints).sqrMagnitude;
						if (magn < minDist)
						{
							minDist = magn;
							closestObjectItem = resultObject;
						}
					}
				}
			}

			return closestObjectItem;
		}

		private static void AddCollider(MeshColliderConf colliderConf, GameObject go)
		{
			Profiler.BeginSample("Action: AddCollider");

			var collider = go.AddComponent<MeshCollider>();
			collider.sharedMesh = colliderConf.Mesh;
			collider.sharedMaterial = colliderConf.Material;
			collider.convex = true;

			Profiler.EndSample();
		}

		private static ColliderSliceResult[] SliceColliders(Plane plane, Collider[] colliders)
		{
			ColliderSliceResult[] results = new ColliderSliceResult[colliders.Length];

			for (int i = 0; i < colliders.Length; i++)
			{
				var collider = colliders[i];

				var colliderB = collider as BoxCollider;
				var colliderS = collider as SphereCollider;
				var colliderC = collider as CapsuleCollider;
				var colliderM = collider as MeshCollider;

				ColliderSliceResult result;
				if (colliderB != null)
				{
					var mesh = Cube.Create(colliderB.size, colliderB.center);
					result = PrepareSliceCollider(collider, mesh, plane);
				}
				else if (colliderS != null)
				{
					var mesh = IcoSphere.Create(colliderS.radius, colliderS.center);
					result = PrepareSliceCollider(collider, mesh, plane);
				}
				else if (colliderC != null)
				{
					var mesh = Capsule.Create(colliderC.radius, colliderC.height, colliderC.direction, colliderC.center);
					result = PrepareSliceCollider(collider, mesh, plane);
				}
				else if (colliderM != null)
				{
					Mesh mesh = UnityEngine.Object.Instantiate(colliderM.sharedMesh);
					result = PrepareSliceCollider(collider, mesh, plane);
				}
				else
					throw new NotSupportedException("Not supported collider type '" + collider.GetType().Name + "'");

				results[i] = result;
			}

			return results;
		}

		public static ColliderSliceResult PrepareSliceCollider(Collider collider, Mesh mesh, Plane plane)
		{
			var result = new ColliderSliceResult();
			IBzSliceAdapter adapter = new BzSliceColliderAdapter(collider.transform);
			SliceConfigurationDto conf = BzSliceConfiguration.GetDefault();
			BzMeshDataDissector meshDissector = new BzMeshDataDissector(mesh, plane, null, adapter, conf, false);

			result.OriginalCollider = collider;
			result.meshDissector = meshDissector;
			result.center = collider.bounds.center;

			return result;
		}

		public class ColliderSliceResult
		{
			public Collider OriginalCollider;
			public Vector3 center;
			public BzMeshDataDissector meshDissector;
			public SliceResult SliceResult;
			public int groupNumberNeg;
			public int groupNumberPos;
		}

		public class MeshColliderConf
		{
			public MeshColliderConf(Mesh mesh, PhysicMaterial material)
			{
				Mesh = mesh;
				Material = material;
			}
			public readonly Mesh Mesh;
			public readonly PhysicMaterial Material;
		}
	}
}

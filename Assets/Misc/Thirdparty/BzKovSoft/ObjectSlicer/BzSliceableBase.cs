using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BzKovSoft.ObjectSlicer.EventHandlers;
using BzKovSoft.ObjectSlicer.Polygon;
using UnityEngine;
using UnityEngine.Profiling;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Base class for sliceable object
	/// </summary>
	[DisallowMultipleComponent]
	public abstract class BzSliceableBase : MonoBehaviour, IBzMeshSlicer
	{
		private readonly List<Task> _tasks = new List<Task>();

		/// <summary>
		/// Material that will be applied to caps
		/// </summary>
		public Material defaultSliceMaterial;
		public bool asynchronously;
		public bool waitIfBusy;
		public bool objectGrouping;

		public async Task<BzSliceTryResult> SliceAsync(Plane plane, object sliceData)
		{
			if (this == null)  // if this component was destroyed
				return null;

			if (waitIfBusy)
			{
				await WaitTasksAsync();
			}
			else if (_tasks.Any(_ => !_.IsCompleted))
			{
				return new BzSliceTryResult() { rejectMessage = "Task is busy." };
			}

			var events = gameObject.GetComponents<IBzObjectSlicedEvent>();
			for (int i = 0; i < events.Length; i++)
			{
				bool valid = events[i].OnSlice(this, plane, sliceData);
				if (!valid) return null;
			}

			var componentManager = GetComponentManager(plane);
			if (componentManager == null)
			{
				return null;
			}

			if (!componentManager.Success)
			{
				return new BzSliceTryResult();
			}

			Renderer[] renderers = GetRenderers(gameObject);
			SliceTryItem[] items = new SliceTryItem[renderers.Length];

			for (int i = 0; i < renderers.Length; i++)
			{
				var renderer = renderers[i];

				var adapterAndMesh = GetAdapterAndMesh(renderer);

				if (adapterAndMesh == null)
					continue;

				Mesh mesh = adapterAndMesh.mesh;
				IBzSliceAdapter adapter = adapterAndMesh.adapter;

				var configuration = renderer.gameObject.GetComponent<BzSliceConfiguration>();
				var confDto = configuration == null ? BzSliceConfiguration.GetDefault() : configuration.GetDto();
				var meshDissector = new BzMeshDataDissector(mesh, plane, renderer.sharedMaterials, adapter, confDto, objectGrouping);
				meshDissector.DefaultSliceMaterial = defaultSliceMaterial;

				SliceTryItem sliceTryItem = new SliceTryItem();
				sliceTryItem.meshRenderer = renderer;
				sliceTryItem.meshDissector = meshDissector;
				items[i] = sliceTryItem;
			}

			SliceTry sliceTry = new SliceTry();
			sliceTry.items = items;
			sliceTry.componentManager = componentManager;
			sliceTry.position = transform.position;

			if (asynchronously)
			{
				await Task.Run(() => Work(sliceTry));
			}
			else
			{
				Work(sliceTry);
			}

			var result = new BzSliceTryResult();
			if (!sliceTry.sliced)
			{
				return result;
			}

			result.sliced = true;
			result.plane = plane;
			result.resultObjects = ApplyChanges(sliceTry);

			Profiler.BeginSample("InvokeEvents");
			var resultGameObjects = result.resultObjects.Select(_ => _.gameObject).ToArray();
			for (int i = 0; i < events.Length; i++)
			{
				events[i].ObjectSliced(gameObject, resultGameObjects, result, sliceData);
			}
			Profiler.EndSample();

			return result;
		}

		public async Task WaitTasksAsync()
		{
			await Task.WhenAll(_tasks.ToArray());
		}

		public void AddTask(Task task)
		{
			_tasks.Add(task);
		}

		/// <summary>
		/// Get the adapter and the mesh instances for a renderer
		/// </summary>
		protected abstract AdapterAndMesh GetAdapterAndMesh(Renderer renderer);

		private void Work(SliceTry sliceTry)
		{
			Profiler.BeginSample("Work");
			var itemMeshes = new List<OneObjectItemMesh>();
			bool somethingOnNeg = false;
			bool somethingOnPos = false;
			for (int i = 0; i < sliceTry.items.Length; i++)
			{
				var sliceTryItem = sliceTry.items[i];
				if (sliceTryItem == null)
				{
					continue;
				}

				var meshDissector = sliceTryItem.meshDissector;

				sliceTryItem.SliceResult = meshDissector.Slice();

				if (sliceTryItem.SliceResult == SliceResult.Neg |
					sliceTryItem.SliceResult == SliceResult.Duplicate |
					sliceTryItem.SliceResult == SliceResult.Sliced)
				{
					somethingOnNeg = true;
				}

				if (sliceTryItem.SliceResult == SliceResult.Pos |
					sliceTryItem.SliceResult == SliceResult.Duplicate |
					sliceTryItem.SliceResult == SliceResult.Sliced)
				{
					somethingOnPos = true;
				}

				if (meshDissector.Configuration.SliceType == SliceType.Slice)
				{
					var meshes = meshDissector.MeshData.Meshes;
					foreach (var mesh in meshes)
					{
						var itemMesh = new OneObjectItemMesh
						{
							Side = mesh.side,
							SliceType = meshDissector.Configuration.SliceType,
							MeshRenderer = sliceTryItem.meshRenderer,
							MeshDissector = meshDissector,
							MeshData = meshDissector.MeshData,
							Mesh = mesh,
						};
						itemMeshes.Add(itemMesh);
					}
				}
				else
				{
					if (somethingOnNeg)
					{
						var itemMesh = new OneObjectItemMesh
						{
							Side = false,
							SliceType = meshDissector.Configuration.SliceType,
							MeshRenderer = sliceTryItem.meshRenderer,
							MeshDissector = meshDissector,
							MeshData = meshDissector.MeshData,
							//Mesh = mesh,
						};
						itemMeshes.Add(itemMesh);
					}
					if (somethingOnPos)
					{
						var itemMesh = new OneObjectItemMesh
						{
							Side = true,
							SliceType = meshDissector.Configuration.SliceType,
							MeshRenderer = sliceTryItem.meshRenderer,
							MeshDissector = meshDissector,
							MeshData = meshDissector.MeshData,
							//Mesh = mesh,
						};
						itemMeshes.Add(itemMesh);
					}
				}

			}

			Profiler.BeginSample("GroupObjects");
			OneObjectItem[] result = GroupObjects(itemMeshes);
			Profiler.EndSample();
			sliceTry.sliced = somethingOnNeg & somethingOnPos;

			if (sliceTry.sliced)
			{
				Profiler.BeginSample("OnSlicedWorkerThread");
				sliceTry.componentManager.OnSlicedWorkerThread(result);
				Profiler.EndSample();
			}

			Profiler.BeginSample("FixObjectGroups");
			sliceTry.resultObjects = FixObjectGroups(result);
			Profiler.EndSample();

			Profiler.EndSample();
		}

		private static OneObjectItem[] FixObjectGroups(OneObjectItem[] result)
		{
			bool changed = false;
			for (int i = 0; i < result.Length; i++)
			{
				var resultA = result[i];
				if (resultA == null)
				{
					continue;
				}

				for (int j = i + 1; j < result.Length; j++)
				{
					var resultB = result[j];
					if (resultB == null)
					{
						continue;
					}

					if (resultA.groupNumber == resultB.groupNumber)
					{
						changed = true;
						foreach (var meshB in resultB.meshes)
						{
							bool inserted = false;
							foreach (var meshA in resultA.meshes)
							{
								if (meshA.MeshData == meshB.MeshData)
								{
									meshA.AddMesh(meshB);
									inserted = true;
									break;
								}
							}

							if (!inserted)
							{
								resultA.meshes.Add(meshB);
							}
						}
						result[j] = null;
					}
				}
			}

			if (changed)
			{
				result = result.Where(_ => _ != null).ToArray();
			}

			return result;
		}

		private OneObjectItem[] GroupObjects(List<OneObjectItemMesh> itemMeshes)
		{
			var objects = new List<OneObjectItem>();
			int counter = 0;

			Profiler.BeginSample("Group 1", this);
			foreach (var itemMesh in itemMeshes)
			{
				if (itemMesh.SliceType != SliceType.Slice)
				{
					continue;
				}

				var candidateObjects = new List<OneObjectItem>();
				foreach (var obj in objects)
				{
					if (obj.side != itemMesh.Side)
					{
						continue;
					}

					bool intersected = false;
					if (objectGrouping)
					{
						foreach (var itemGroupedItem in obj.meshes)
						{
							intersected = FindIntersectionByTriangles(itemMesh.Mesh.triangles, itemGroupedItem.Mesh.triangles);

							if (intersected)
							{
								break;
							}
						}
					}
					else
					{
						intersected = true;
					}

					if (intersected)
					{
						candidateObjects.Add(obj);
					}
				}

				if (candidateObjects.Any())
				{
					var first = candidateObjects.First();
					var others = candidateObjects.Skip(1).ToList();
					first.meshes.Add(itemMesh);
					foreach (var other in others)
					{
						objects.Remove(other);
						first.meshes.AddRange(other.meshes);
					}
				}
				else
				{
					var oneObjItem = new OneObjectItem
					{
						groupNumber = ++counter,
						side = itemMesh.Side,
						meshes = new List<OneObjectItemMesh> { itemMesh },
					};
					objects.Add(oneObjItem);
				}
			}
			Profiler.EndSample();

			Profiler.BeginSample("Group 2");
			foreach (var obj in objects)
			{
				var groups = obj
					.meshes
					.GroupBy(_ => _.MeshData)
					.Where(_ => _.Count() > 1)
					.ToList();

				foreach (var group in groups)
				{
					var first = group.First();
					var others = group.Skip(1).ToList();

					foreach (var other in others)
					{
						obj.meshes.Remove(other);
						first.AddMesh(other);
					}
				}

				// calculate center
				foreach (var mesh in obj.meshes)
				{
					float volume = BzSlicerHelper.VolumeOfMesh(mesh.Mesh.triangles, out var centerOfMass, out var centerOfPoints);

					mesh.Volume = volume;
					mesh.CenterOfMass = centerOfMass;
					mesh.CenterOfPoints = centerOfPoints;
				}
			}
			Profiler.EndSample();

			Profiler.BeginSample("Group 3");
			foreach (var itemMesh in itemMeshes)
			{
				if (itemMesh.SliceType == SliceType.Slice)
				{
					continue;
				}

				var meshCenter = itemMesh.MeshDissector.Adapter.GetObjectCenterInWorldSpace();

				OneObjectItem candidateObject = null;
				OneObjectItemMesh candidateMesh = null;
				float minDistance = float.MaxValue;

				foreach (var obj in objects)
				{

					if (obj.side != itemMesh.Side)
					{
						continue;
					}

					foreach (var mesh in obj.meshes)
					{
						if (mesh.SliceType != SliceType.Slice)
						{
							continue;
						}

						float newDistance = (meshCenter - mesh.CenterOfMass).sqrMagnitude;

						if (newDistance < minDistance)
						{
							minDistance = newDistance;
							candidateObject = obj;
							candidateMesh = mesh;
						}
					}
				}

				itemMesh.CenterOfMass = candidateMesh.CenterOfMass;
				itemMesh.CenterOfPoints = candidateMesh.CenterOfPoints;
				candidateObject.meshes.Add(itemMesh);
			}
			Profiler.EndSample();

			return objects.ToArray();
		}

		private static bool FindIntersectionByTriangles(List<BzTriangle> mesh1, List<BzTriangle> mesh2)
		{
			var bounds1 = new BzTriangleBound[mesh1.Count];
			var bounds2 = new BzTriangleBound[mesh2.Count];
			for (int i = 0; i < mesh1.Count; i++)
			{
				BzTriangle tr = mesh1[i];
				bounds1[i] = new BzTriangleBound(ref tr);
			}
			for (int i = 0; i < mesh2.Count; i++)
			{
				BzTriangle tr = mesh2[i];
				bounds2[i] = new BzTriangleBound(ref tr);
			}

			for (int i = 0; i < bounds1.Length; i++)
			{
				BzTriangle tr1 = mesh1[i];
				BzTriangleBound b1 = bounds1[i];
				for (int j = 0; j < bounds2.Length; j++)
				{
					BzTriangleBound b2 = bounds2[j];

					bool triagleAABB =
						b1.minX < b2.maxX & b1.maxX > b2.minX &
						b1.minY < b2.maxY & b1.maxY > b2.minY &
						b1.minZ < b2.maxZ & b1.maxZ > b2.minZ;
					if (triagleAABB)
					{
						BzTriangle tr2 = mesh2[j];
						if (TriTriOverlap.TriTriIntersect(ref tr1, ref tr2))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		private readonly struct BzTriangleBound
		{
			public readonly float minX;
			public readonly float maxX;
			public readonly float minY;
			public readonly float maxY;
			public readonly float minZ;
			public readonly float maxZ;

			public BzTriangleBound(ref BzTriangle tr1)
			{
				minX = tr1.v1.x < tr1.v2.x ? tr1.v1.x : tr1.v2.x; minX = tr1.v3.x < minX ? tr1.v3.x : minX;
				maxX = tr1.v1.x > tr1.v2.x ? tr1.v1.x : tr1.v2.x; maxX = tr1.v3.x > maxX ? tr1.v3.x : maxX;
				minY = tr1.v1.y < tr1.v2.y ? tr1.v1.y : tr1.v2.y; minY = tr1.v3.y < minY ? tr1.v3.y : minY;
				maxY = tr1.v1.y > tr1.v2.y ? tr1.v1.y : tr1.v2.y; maxY = tr1.v3.y > maxY ? tr1.v3.y : maxY;
				minZ = tr1.v1.z < tr1.v2.z ? tr1.v1.z : tr1.v2.z; minZ = tr1.v3.z < minZ ? tr1.v3.z : minZ;
				maxZ = tr1.v1.z > tr1.v2.z ? tr1.v1.z : tr1.v2.z; maxZ = tr1.v3.z > maxZ ? tr1.v3.z : maxZ;
			}
		}

		private BzSlicerTryResultObject[] ApplyChanges(SliceTry sliceTry)
		{
			Profiler.BeginSample("ApplyChanges");

			var resultObjects = sliceTry.resultObjects;
			GameObject[] newObjects = GetNewObjects(resultObjects);
			for (int i = 0; i < resultObjects.Length; i++)
			{
				OneObjectItem resultObject = resultObjects[i];
				resultObject.newObject = newObjects[i];

				foreach (var mesh in resultObject.meshes)
				{
					var newMeshRenderer = BzSlicerHelper.GetSameComponentForDuplicate(mesh.MeshRenderer, gameObject, resultObject.newObject);
					mesh.MeshRenderer = newMeshRenderer;
				}
			}

			Profiler.BeginSample("ComponentManager.OnSlicedMainThread");
			sliceTry.componentManager.OnSlicedMainThread(sliceTry.resultObjects);
			Profiler.EndSample();

			var result = new BzSlicerTryResultObject[resultObjects.Length];
			for (int i = 0; i < resultObjects.Length; i++)
			{
				OneObjectItem resultObject = resultObjects[i];
				GameObject newObject = resultObject.newObject;

				foreach (var mesh in resultObject.meshes)
				{
					if (mesh.SliceType == SliceType.Slice)
					{
						var generatedMesh = mesh.MeshData.GenerateMeshes(mesh.Mesh);
						mesh.MeshDissector.Adapter.RebuildMesh(generatedMesh.mesh, generatedMesh.materials, mesh.MeshRenderer);
					}
				}

				var renderers = GetRenderers(newObject)
					.Where(_ => !resultObject.meshes.Any(r => r.MeshRenderer == _))
					.ToList();

				foreach (var renderer in renderers)
				{
					DeleteRenderer(renderer);
				}

				result[i] = new BzSlicerTryResultObject
				{
					gameObject = newObject,
					side = resultObject.side,
					meshes = GetItemResult(resultObject),
				};
			}

			Profiler.EndSample();
			return result;
		}

		private static BzMeshSliceResult[] GetItemResult(OneObjectItem oneObjectItem)
		{
			var itemResults = new BzMeshSliceResult[oneObjectItem.meshes.Count];

			for (int i = 0; i < oneObjectItem.meshes.Count; i++)
			{
				var mesh = oneObjectItem.meshes[i];
				var itemResult = new BzMeshSliceResult();
				itemResult.renderer = mesh.MeshRenderer;
				itemResult.sliceType = mesh.MeshDissector.Configuration.SliceType;

				if (mesh.MeshDissector.Configuration.SliceType == SliceType.Slice)
				{
					if (mesh.MeshDissector.Configuration.CreateCap)
					{
						var sliceEdgeResult = new BzSliceEdgeResult[mesh.MeshDissector.CapResult.Count];
						for (int j = 0; j < sliceEdgeResult.Length; j++)
						{
							var edgeResult = MakeEdgeResult(mesh.MeshDissector.CapResult[j]);
							sliceEdgeResult[j] = edgeResult;
						}
						itemResult.sliceEdges = sliceEdgeResult;
					}
				}
				itemResults[i] = itemResult;
			}

			return itemResults;
		}

		private static BzSliceEdgeResult MakeEdgeResult(PolyMeshData polyMeshData)
		{
			var result = new BzSliceEdgeResult();
			result.vertices = polyMeshData.vertices;
			result.normals = polyMeshData.normals;
			result.boneWeights = polyMeshData.boneWeights;
			return result;
		}

		/// <summary>
		/// Method must return a new gameObject for each income object
		/// </summary>
		protected virtual GameObject[] GetNewObjects(OneObjectItem[] objects)
		{
			int negCount = 0;
			int posCount = 0;
			string originalName = this.gameObject.name;
			var result = new GameObject[objects.Length];

			result[0] = gameObject;
			for (int i = 1; i < objects.Length; i++)
			{
				result[i] = Instantiate(this.gameObject, this.gameObject.transform.parent);
			}

			for (int i = 0; i < objects.Length; i++)
			{
				var obj = objects[i];
				string newObjName = originalName;
				if (obj.side)
				{
					newObjName += "_pos_" + (++negCount).ToString(CultureInfo.InvariantCulture);
				}
				else
				{
					newObjName += "_neg_" + (++posCount).ToString(CultureInfo.InvariantCulture);
				}

				result[i].name = newObjName;
			}

			return result;
		}

		private static void DeleteRenderer(Renderer renderer)
		{
			GameObject.Destroy(renderer);
			var mf = renderer.gameObject.GetComponent<MeshFilter>();
			if (mf != null)
			{
				GameObject.Destroy(mf);
			}
		}

		/// <summary>
		/// Prepare component manager that will be used for slicing
		/// </summary>
		protected virtual IComponentManager GetComponentManager(Plane plane)
		{
			// the colliders that will participate in a slicing
			var colliders = gameObject.GetComponentsInChildren<Collider>();

			// componentManager: this class will manage components on sliced objects
			return new StaticComponentManager(gameObject, plane, colliders);
		}

		private Renderer[] GetRenderers(GameObject gameObject)
		{
			return gameObject.GetComponentsInChildren<Renderer>();
		}

		public override string ToString()
		{
			// prevent from accessing the name in debug mode.
			return GetType().Name;
		}

		protected class AdapterAndMesh
		{
			public IBzSliceAdapter adapter;
			public Mesh mesh;
		}
	}
}
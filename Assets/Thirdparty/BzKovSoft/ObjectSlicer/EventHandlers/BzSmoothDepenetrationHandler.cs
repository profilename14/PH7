using System.Threading.Tasks;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.EventHandlers
{
	/// <summary>
	/// After the slice some collider could intersect each other and jump apart. This script designed to avoid this effect 
	/// </summary>
	[DisallowMultipleComponent]
	public class BzSmoothDepenetration : MonoBehaviour, IBzObjectSlicedEvent
	{
		public bool OnSlice(IBzMeshSlicer meshSlicer, Plane plane, object sliceData)
		{
			return true;
		}

		public void ObjectSliced(GameObject original, GameObject[] resultObjects, BzSliceTryResult result, object sliceData)
		{
			var task = ObjectSlicedAsync(result);

			foreach (var resultObject in result.resultObjects)
			{
				var slicer = resultObject.gameObject.GetComponent<IBzMeshSlicer>();
				slicer.AddTask(task);
			}
		}

		public async Task ObjectSlicedAsync(BzSliceTryResult result)
		{
			var items = new ObjectItem[result.resultObjects.Length];
			for (int i = 0; i < result.resultObjects.Length; ++i)
			{
				var item = new ObjectItem();
				item.result = result.resultObjects[i].gameObject;
				item.rigids = item.result.GetComponentsInChildren<Rigidbody>();
				item.maxVelocities = new float[item.rigids.Length];
				for (int j = 0; j < item.rigids.Length; j++)
				{
					var rigid = item.rigids[j];
					item.maxVelocities[j] = rigid.maxDepenetrationVelocity;
					rigid.maxDepenetrationVelocity = 0.1f;
				}

				items[i] = item;
			}

			await Task.Delay(1000);

			for (int i = 0; i < items.Length; ++i)
			{
				var item = items[i];

				for (int j = 0; j < item.rigids.Length; j++)
				{
					var rigid = item.rigids[j];
					if (rigid == null)
						continue;

					float maxVel = item.maxVelocities[j];
					rigid.maxDepenetrationVelocity = maxVel;
				}
			}
		}

		struct ObjectItem
		{
			public GameObject result;
			public Rigidbody[] rigids;
			public float[] maxVelocities;
		}
	}
}

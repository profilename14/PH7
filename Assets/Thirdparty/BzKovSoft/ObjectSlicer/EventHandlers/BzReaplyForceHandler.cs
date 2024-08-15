using System.Threading.Tasks;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.EventHandlers
{
	/// <summary>
	/// Reapplying velocity and angularVelocity from the original
	/// </summary>
	[DisallowMultipleComponent]
	public class BzReaplyForceHandler : MonoBehaviour, IBzObjectSlicedEvent
	{
		public bool OnSlice(IBzMeshSlicer meshSlicer, Plane plane, object sliceData)
		{
			return true;
		}

		public void ObjectSliced(GameObject original, GameObject[] resultObjects, BzSliceTryResult result, object sliceData)
		{
			var task = ObjectSlicedAsync(original, result);

			foreach (var resultObject in result.resultObjects)
			{
				var slicer = resultObject.gameObject.GetComponent<IBzMeshSlicer>();
				slicer.AddTask(task);
			}
		}

		public async Task ObjectSlicedAsync(GameObject original, BzSliceTryResult result)
		{
			// we need to wait one frame to allow destroyed component to be destroyed.
			//returning null will make it wait 1 frame
			await Task.Yield();

			var origRigid = original.GetComponent<Rigidbody>();
			if (origRigid == null)
				return;

			foreach (var resultObject in result.resultObjects)
			{
				var rigid = resultObject.gameObject.GetComponent<Rigidbody>();
				if (rigid == null)
					continue;

				rigid.angularVelocity = origRigid.angularVelocity;
				rigid.velocity = origRigid.velocity;
			}
		}
	}
}

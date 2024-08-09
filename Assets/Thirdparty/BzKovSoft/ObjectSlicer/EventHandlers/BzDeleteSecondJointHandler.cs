using UnityEngine;

namespace BzKovSoft.ObjectSlicer.EventHandlers
{
	/// <summary>
	/// The script deletes the Joint from the farthest object from the anchor.
	/// </summary>
	[DisallowMultipleComponent]
	public class BzDeleteSecondJointHandler : MonoBehaviour, IBzObjectSlicedEvent
	{
		public bool OnSlice(IBzMeshSlicer meshSlicer, Plane plane, object sliceData)
		{
			return true;
		}

		public void ObjectSliced(GameObject original, GameObject[] resultObjects, BzSliceTryResult result, object sliceData)
		{
			if (!original.TryGetComponent<Joint>(out var oJoint))
				return;

			GameObject minDistGo = null;
			float minDist = float.MaxValue;

			foreach (var resultObject in result.resultObjects)
			{
				Mesh mesh = resultObject.gameObject.GetComponent<MeshFilter>().sharedMesh;
				if (mesh == null)
					continue;

				float dist = (oJoint.anchor - mesh.bounds.center).sqrMagnitude;
				if (minDist > dist)
				{
					minDist = dist;
					minDistGo = resultObject.gameObject;
				}
			}

			if (minDistGo == null)
				return;

			foreach (var resultObject in resultObjects)
			{
				if (resultObject != minDistGo)
				{
					Destroy(resultObject.GetComponent<Joint>());
				}
			}
		}
	}
}

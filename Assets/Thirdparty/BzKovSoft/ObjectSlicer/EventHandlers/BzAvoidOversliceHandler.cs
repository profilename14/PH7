using UnityEngine;

namespace BzKovSoft.ObjectSlicer.EventHandlers
{
	/// <summary>
	/// Prevents too offen cuts. There must be a delay between them.
	/// </summary>
	[DisallowMultipleComponent]
	public class BzAvoidOversliceHandler : MonoBehaviour, IBzObjectSlicedEvent
	{
#pragma warning disable 0649
		[HideInInspector]
		[SerializeField]
		float _lastSliceTime = float.MinValue;
		[SerializeField]
		int _maxSliceCount = 3;
#pragma warning restore 0649

		/// <summary>
		/// If your code do not use SliceId, it can relay on delay between last slice and new one.
		/// If real delay is less than this value, slice will be ignored
		/// </summary>
		public float delayBetweenSlices = 1f;

		public bool OnSlice(IBzMeshSlicer meshSlicer, Plane plane, object sliceData)
		{
			if (_maxSliceCount == 0)
				return false;

			float currentSliceTime = Time.time;

			// we must prevent slicing same object if the _delayBetweenSlices was not exceeded
			if (_lastSliceTime + delayBetweenSlices > currentSliceTime)
			{
				return false;
			}

			_lastSliceTime = currentSliceTime;

			return true;
		}

		public void ObjectSliced(GameObject original, GameObject[] resultObjects, BzSliceTryResult result, object sliceData)
		{
			--_maxSliceCount;

			foreach (var resultObject in result.resultObjects)
			{
				var resultSlicer = resultObject.gameObject.GetComponent<BzAvoidOversliceHandler>();
				resultSlicer._maxSliceCount = _maxSliceCount;
			}
		}
	}
}

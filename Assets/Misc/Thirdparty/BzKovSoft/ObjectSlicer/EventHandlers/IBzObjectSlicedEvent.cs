using UnityEngine;

namespace BzKovSoft.ObjectSlicer.EventHandlers
{
	/// <summary>
	/// Object Slicer event handler interface
	/// </summary>
	public interface IBzObjectSlicedEvent
	{
		/// <summary>
		/// Called when a "Slice" is called
		/// </summary>
		/// <returns>If false, slice will be stopped</returns>
		bool OnSlice(IBzMeshSlicer meshSlicer, Plane plane, object sliceData);
		/// <summary>
		/// called when the object was successfully sliced
		/// </summary>
		void ObjectSliced(GameObject original, GameObject[] resultObjects, BzSliceTryResult result, object sliceData);
	}
}

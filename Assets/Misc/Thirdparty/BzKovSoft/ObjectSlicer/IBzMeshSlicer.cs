using System;
using System.Threading.Tasks;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Interface that implements mesh separation logic
	/// </summary>
	public interface IBzMeshSlicer
	{
		/// <summary>
		/// Start slicing the object
		/// </summary>
		/// <param name="plane">Plane by which you are going to slice</param>
		Task<BzSliceTryResult> SliceAsync(Plane plane, object sliceData = null);
		/// <summary>
		/// Wait all internal tasks to be completed
		/// </summary>
		Task WaitTasksAsync();
		/// <summary>
		/// Add a task to a slicer waiting queue
		/// </summary>
		/// <param name="task">Task to wait</param>
		void AddTask(Task task);
	}

	[Obsolete("Use IBzMeshSlicer interface", true)]
	public interface IBzSliceable
	{
	}

	[Obsolete("Use IBzMeshSlicer interface", true)]
	public interface IBzSliceableAsync
	{

	}
}

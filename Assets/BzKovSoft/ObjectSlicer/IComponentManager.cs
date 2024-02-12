namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Manage components for sliced objects
	/// </summary>
	public interface IComponentManager
	{
		bool Success { get; }

		/// <summary>
		/// Asynchronous call
		/// </summary>
		void OnSlicedWorkerThread(OneObjectItem[] resultObjects);

		/// <summary>
		/// Synchronous call
		/// </summary>
		void OnSlicedMainThread(OneObjectItem[] resultObjects);
	}
}

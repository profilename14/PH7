using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Cut request
	/// </summary>
	public class SliceTry
	{
		public OneObjectItem[] resultObjects;
		public SliceTryItem[] items;
		public IComponentManager componentManager;
		public bool sliced;
		public Vector3 position;
	}

	/// <summary>
	/// Cut request item
	/// </summary>
	public class SliceTryItem
	{
		public BzMeshDataDissector meshDissector;
		public Renderer meshRenderer;
		public SliceResult SliceResult;
	}
}
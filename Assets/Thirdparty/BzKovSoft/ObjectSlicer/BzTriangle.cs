using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// One triangle from a mesh
	/// </summary>
	public class BzTriangle
	{
		public readonly int i1;
		public readonly int i2;
		public readonly int i3;
		public readonly Vector3 v1;
		public readonly Vector3 v2;
		public readonly Vector3 v3;
		public readonly int subMeshIndex;
		public readonly bool side;

		/// <summary>
		/// Create triangle from 3 indexes of a mesh
		/// </summary>
		public BzTriangle(int i1, int i2, int i3, Vector3 v1, Vector3 v2, Vector3 v3, int subMeshIndex, bool side)
		{
			this.i1 = i1;
			this.i2 = i2;
			this.i3 = i3;
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
			this.subMeshIndex = subMeshIndex;
			this.side = side;
		}
	}
}

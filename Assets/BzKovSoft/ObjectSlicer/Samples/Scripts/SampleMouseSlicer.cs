using UnityEngine;

namespace BzKovSoft.ObjectSlicer.Samples
{
	/// <summary>
	/// Mouse raycast to the object and slice it if hit
	/// </summary>
	public class SampleMouseSlicer : MonoBehaviour
	{
		async void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				// if left mouse clicked, try slice this object

				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

				for (int i = 0; i < hits.Length; i++)
				{
					var sliceableA = hits[i].transform.GetComponentInParent<IBzMeshSlicer>();

					Vector3 direction = Vector3.Cross(ray.direction, Camera.main.transform.right);
					Plane plane = new Plane(direction, ray.origin);

					if (sliceableA != null)
					{
						await sliceableA.SliceAsync(plane);
					}
				}
			}
		}
	}
}
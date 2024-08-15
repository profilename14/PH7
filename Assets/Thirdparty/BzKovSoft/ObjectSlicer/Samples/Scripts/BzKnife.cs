using UnityEngine;

namespace BzKovSoft.ObjectSlicer.Samples
{
	/// <summary>
	/// The script must be attached to a GameObject that have collider marked as a "IsTrigger".
	/// </summary>
	public class BzKnife : MonoBehaviour
	{
		Vector3 _prevPos;
		Vector3 _pos;

		[SerializeField]
		private Vector3 _origin = Vector3.down;

		[SerializeField]
		private Vector3 _direction = Vector3.up;

		private void Update()
		{
			_prevPos = _pos;
			_pos = transform.position;
		}

		/// <summary>
		/// Origin of the knife
		/// </summary>
		public Vector3 Origin
		{
			get
			{
				Vector3 localShifted = transform.InverseTransformPoint(transform.position) + _origin;
				return transform.TransformPoint(localShifted);
			}
		}

		/// <summary>
		/// The direction the knife is pointed to
		/// </summary>
		public Vector3 BladeDirection { get { return transform.rotation * _direction.normalized; } }
		/// <summary>
		/// The nnife moving direction
		/// </summary>
		public Vector3 MoveDirection { get { return (_pos - _prevPos).normalized; } }

		void OnDrawGizmosSelected()
		{
			var color = Gizmos.color;
			var direction = transform.rotation * _direction;
			var from = Origin;
			var to = Origin + direction;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(from, to);
			Gizmos.DrawSphere(from, 0.06f);
			DrawArrowEnd(from, direction);
			Gizmos.color = color;
		}

		private static void DrawArrowEnd(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
		{
			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back;
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back;
			Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
			Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;
			right *= arrowHeadLength;
			left *= arrowHeadLength;
			up *= arrowHeadLength;
			down *= arrowHeadLength;

			Vector3 center = pos + direction;
			Gizmos.DrawRay(center, right);
			Gizmos.DrawRay(center, left);
			Gizmos.DrawRay(center, up);
			Gizmos.DrawRay(center, down);
			
			Gizmos.DrawLine(center + right, center + left);
			Gizmos.DrawLine(center + up, center + down);
		}

		async void OnTriggerEnter(Collider other)
		{
			var slicer = other.GetComponentInParent<IBzMeshSlicer>();
			if (slicer == null)
			{
				return;
			}

			Vector3 point = GetCollisionPoint();
			Vector3 normal = Vector3.Cross(MoveDirection, BladeDirection);
			Plane plane = new Plane(normal, point);

			await slicer.SliceAsync(plane);
		}

		private Vector3 GetCollisionPoint()
		{
			Vector3 distToObject = transform.position - Origin;
			Vector3 proj = Vector3.Project(distToObject, BladeDirection);

			Vector3 collisionPoint = Origin + proj;
			return collisionPoint;
		}
	}
}

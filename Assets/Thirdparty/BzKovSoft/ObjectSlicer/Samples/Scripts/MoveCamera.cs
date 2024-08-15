using UnityEngine;

namespace BzKovSoft.ObjectSlicer.Samples
{
	/// <summary>
	/// Camera movement controller
	/// </summary>
	public class MoveCamera : MonoBehaviour
	{
		/// <summary>
		/// Speed of camera turning when mouse moves in along an axis
		/// </summary>
		public float TurnSpeed = 4.0f;
		/// <summary>
		/// Speed of the camera going back and forth
		/// </summary>
		public float MoveSpeed = 2.0f;

		private float yaw = 0f;
		private float pitch = 0f;

		void Update()
		{
			var camera = Camera.main.gameObject.transform;

			Vector3 move = Vector3.zero;
			if (Input.GetKey(KeyCode.W))
				move += MoveSpeed * Time.deltaTime * Vector3.forward;
			if (Input.GetKey(KeyCode.S))
				move += MoveSpeed * Time.deltaTime * Vector3.back;
			if (Input.GetKey(KeyCode.A))
				move += MoveSpeed * Time.deltaTime * Vector3.left;
			if (Input.GetKey(KeyCode.D))
				move += MoveSpeed * Time.deltaTime * Vector3.right;
			if (Input.GetKey(KeyCode.Q))
				move += MoveSpeed * Time.deltaTime * Vector3.down;
			if (Input.GetKey(KeyCode.E))
				move += MoveSpeed * Time.deltaTime * Vector3.up;

			if (Input.GetKey(KeyCode.LeftShift))
				move *= 5;

			if (Mathf.Abs(move.sqrMagnitude) > Mathf.Epsilon)
				camera.Translate(move, Space.Self);

			if (Input.GetMouseButton(1))
			{
				yaw += Input.GetAxis("Mouse X");
				pitch -= Input.GetAxis("Mouse Y");
				camera.eulerAngles = new Vector3(TurnSpeed * pitch, TurnSpeed * yaw, 0.0f);
			}
		}
	}
}
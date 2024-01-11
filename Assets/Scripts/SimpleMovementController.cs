using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovementController : MonoBehaviour
{
    // Movement speed for the player.
    const float DEFAULT_SPEED = 11.0f;
    float speed = DEFAULT_SPEED;
    Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
      Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
      input.Normalize();

      Vector3 movement = new Vector3(input.x * speed * Time.deltaTime, 0, input.z * speed * Time.deltaTime);

      rigidbody.MovePosition(transform.position + movement);
    }
}

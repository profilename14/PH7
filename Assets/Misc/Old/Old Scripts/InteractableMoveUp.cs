using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableMoveUp : ActivatedObject
{
    float moveTimer = 0.0f;
    public float maxTime = 4.0f;
    public bool isMoving = false;
    public float speed = 1.5f;

    // Start is called before the fi+rst frame update
    void Start()
    {
      moveTimer = maxTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving) {
          if (moveTimer > 0) {
            transform.position = transform.position + new Vector3(0, speed * Time.deltaTime, 0);
            moveTimer -= Time.deltaTime;
          } else {
            isMoving = false;
          }
        }
    }

    public override void Activate()
    {
        isMoving = true;
    }
}

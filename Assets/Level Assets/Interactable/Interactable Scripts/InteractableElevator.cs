using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableElevator : ActivatedObject
{
    float moveTimer = 0.0f;
    public float maxTime = 4.0f;
    public bool isMovingUp = false;
    public float speed = 1.5f;
    float hitTimer = 0.0f;
    bool enabled = false;

    // Start is called before the fi+rst frame update
  void Start()
    {
      //moveTimer = maxTime;
    }

  // Update is called once per frame
  void Update()
  {
    if (!enabled)
    {
      return;
    }
    
    if (hitTimer > 0)
    {
      hitTimer -= Time.deltaTime;
    }

    if (isMovingUp)
    {
      if (moveTimer < maxTime)
      {
        transform.position = transform.position + new Vector3(0, speed * Time.deltaTime, 0);
        moveTimer += Time.deltaTime;
      }
      else
      {
        Activate();
      }

    }
    else
    {
      if (moveTimer > 0)
      {
        transform.position = transform.position + new Vector3(0, -speed * Time.deltaTime, 0);
        moveTimer -= Time.deltaTime;

      }
      else
      {
        Activate();
      }

    }
    
  }

  public override void Activate()
  {
    enabled = true;
    
    if (hitTimer > 0) {
      return;
    } else {
      hitTimer = 0.175f;
    }
    if (isMovingUp) {
      isMovingUp = false;
    } else {
      isMovingUp = true;
    }
  }


}

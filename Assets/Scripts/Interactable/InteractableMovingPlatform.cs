using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableMovingPlatform : ActivatedObject
{
    float moveTimer = 0.0f;
    public float maxTime = 4.0f;
    public bool isMovingAway = false; // from the starting point
    public float speed = 1.5f;
    float hitTimer = 0.0f;
    bool enabled = false;
    public Vector3 direction; // should be a unit vector
    public bool singleTime = false;

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

    if (isMovingAway)
    {
      if (moveTimer < maxTime)
      {
        transform.position = transform.position + direction * speed * Time.deltaTime;
        moveTimer += Time.deltaTime;
      }
      else
      {
        if (!singleTime)
        {
            Activate();
        }
        
      }

    }
    else
    {
      if (moveTimer > 0)
      {
        transform.position = transform.position - direction * speed * Time.deltaTime;
        moveTimer -= Time.deltaTime;

      }
      else
      {
        if (!singleTime)
        {
            Activate();
        }
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
    if (isMovingAway) {
      isMovingAway = false;
    } else {
      isMovingAway = true;
    }
  }


}

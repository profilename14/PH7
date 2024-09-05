using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : CharacterState
{
    [SerializeField]
    private Vector2 moveDir;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateMovement(Vector2 movement)
    {
        moveDir = movement;
    }
}

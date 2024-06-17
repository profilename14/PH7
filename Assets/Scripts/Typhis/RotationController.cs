using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Made by Jasper Fadden
// REsponsible for hving Typhis look at the mouse.


public class RotationController : MonoBehaviour
{
  // Used to store what angle clockwise the player's mouse is relative to the player.
  public float angle;
  public Vector3 directionVec;
  Vector3 mousePosition;

  private float distanceMouseIs = 0.0f;
  private Vector3 Direction3D;

  bool isControllerUsed = false;
  public bool canTurn = true;
  public bool isFacingMouse = false;

  private Vector3 camForward;
  private Vector3 camRight;

  void Awake() {
    var cam = Camera.main;

    camForward = cam.transform.forward;
    camRight = cam.transform.right;

    camForward.y = 0;
    camRight.y = 0;

    camForward.Normalize();
    camRight.Normalize();


  }

  // Rotation controller handles game manager stuff
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.C))
    {
      if (!GameManager.isControllerUsed)
      {
        GameManager.isControllerUsed = true;
      }
      else
      {
        GameManager.isControllerUsed = false;
      }
    }

    /*if (Input.GetButton("Jump") && !Input.GetKeyDown(KeyCode.Space)) {
      GameManager.isControllerUsed = true;
    }*/

        if (Input.GetKeyDown(KeyCode.K))
    {
      if (!GameManager.isScreenshakeEnabled)
      {
        GameManager.isScreenshakeEnabled = true;
      }
      else
      {
        GameManager.isScreenshakeEnabled = false;
      }
    }

    if (GameManager.slowTimer > 0)
    {
      GameManager.slowTimer -= Time.deltaTime;

    }
    if (GameManager.slowTimer <= 0 && Time.timeScale < 1f)
    {
      Time.timeScale = 1f;
    }
  }

  public Vector3 GetRotationDirection() {
    Vector3 dir;
    if (GameManager.isControllerUsed) {
      dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
      dir.Normalize();
    } else {
      float h = Input.mousePosition.x - Screen.width / 2;
      float v = Input.mousePosition.y - Screen.height / 2;
      dir = new Vector3(h, 0, v);
      dir.Normalize();
    }

    dir = Quaternion.Euler(0, -45+180, 0) * dir;
    return dir;
  }

  public void snapToCurrentAngle() { // calls fixed update code, to be called between attacks and frames
      if (GameManager.isControllerUsed)
      {
          /*Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
          angle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
          directionVec = camForward * input.x + camRight * input.y;
          directionVec.Normalize();

          if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2 || Mathf.Abs(Input.GetAxis("Vertical")) > 0.2)
          {
              transform.rotation = Quaternion.Euler(0, angle - 45, 0);
          }*/

          Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
          angle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
          directionVec = camForward * input.x + camRight * input.y;
          directionVec.Normalize();

          if ( Quaternion.Angle( transform.rotation, Quaternion.Euler(0, angle - 90 -45, 0) ) == 180f ) {
            angle -= 90;
            //Debug.Log ("1 frame Flip");
          }

          if (input.x != 0 || input.y != 0)
          {
              transform.rotation = Quaternion.Euler(0, angle - 90 -45, 0);
          }

      }
      else
      {
          /*float h = Input.mousePosition.x - Screen.width / 2;
          float v = Input.mousePosition.y - Screen.height / 2;
          directionVec = new Vector3(h, 0, v);


          directionVec.Normalize();

          float angle = -Mathf.Atan2(v, h) * Mathf.Rad2Deg;

          transform.rotation = Quaternion.Euler(0, angle , 0);*/

          Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
          angle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
          directionVec = camForward * input.x + camRight * input.y;
          directionVec.Normalize();

          if ( Quaternion.Angle( transform.rotation, Quaternion.Euler(0, angle - 90 -45, 0) ) == 180f ) {
            angle -= 90;
            //Debug.Log ("1 frame Flip");
          }

          if (input.x != 0 || input.y != 0)
          {
              transform.rotation = Quaternion.Euler(0, angle - 90 -45, 0);
          }


      }
  }

  public void snapToCurrentMouseAngle() {
    if (GameManager.isControllerUsed) {
      return;
      Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
      float angle2 = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
      directionVec = camForward * input.x + camRight * input.y;
      directionVec.Normalize();

      if (Quaternion.Angle(transform.rotation, Quaternion.Euler(0, angle2 - 90 - 45, 0)) == 180f)
      {
        angle2 -= 90;
        //Debug.Log ("1 frame Flip");
      }

      if (input.x != 0 || input.y != 0)
      {
        transform.rotation = Quaternion.Euler(0, angle2 - 90 - 45, 0);
      }
      return;
    }
    float h = Input.mousePosition.x - Screen.width / 2;
    float v = Input.mousePosition.y - Screen.height / 2;
    directionVec = new Vector3(h, 0, v);


    directionVec.Normalize();

    float angle = -Mathf.Atan2(v, h) * Mathf.Rad2Deg;

    //directionVec = Quaternion.Euler(0, -45, 0) * directionVec
    transform.rotation = Quaternion.Euler(0, angle - 45, 0);
  }


  void FixedUpdate() {
        /*mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePosition - transform.position;
        // This is used to detect where the player should be facing.
        angle = Vector2.SignedAngle(Vector2.down, direction) + 270;

        transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);*/
        if (PlayerCombatController.currentState == PlayerCombatController.PlayerState.Idle
         || PlayerCombatController.currentState == PlayerCombatController.PlayerState.ChargeSpinslash)
        {
            if (!isFacingMouse) {
              snapToCurrentAngle();

              canTurn = true;
            } else {
              snapToCurrentMouseAngle();
              canTurn = true;
            }

         } else {
            if (canTurn == true) {
              //snapToCurrentMouseAngle();
            }
            canTurn = false;
         }


  }


}

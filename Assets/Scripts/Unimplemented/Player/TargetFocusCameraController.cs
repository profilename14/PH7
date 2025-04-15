using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class TargetFocusCameraController : MonoBehaviour
    {
        [SerializeField] private float idleDuration;
        [SerializeField] private float leadSpeed;
        [SerializeField] private float leadMaxDistance;
        [SerializeField] private float constMove;
        [SerializeField] private float targetOffsetX;
        [SerializeField] private float targetOffsetZ;
        [SerializeField] private float targetOffsetY = 42.5f;
        private Camera managedCamera;
        [SerializeField]
        private GameObject target;
        private RotationController rotation;

        private void Awake()
        {
            this.managedCamera = this.gameObject.GetComponent<Camera>();
            if (!target)
            {
                target = new GameObject();
            }
            rotation = target.GetComponent<RotationController>();
        }

        //Use the LateUpdate message to avoid setting the camera's position before
        //GameObject locations are finalized.
        void LateUpdate()
        {
            var targetPosition = target.transform.position;
            targetPosition.x += targetOffsetX;
            targetPosition.z += targetOffsetZ;
            targetPosition.y += targetOffsetY;
            var cameraPosition = managedCamera.transform.position;

            var speed = rotation.GetRotationDirection() * leadMaxDistance;

            //if ( (speed.x < 0.1 || speed.x > -0.1) && (speed.z < 0.1 || speed.z > -0.1) ) {

              // Return to the origin from the max distance over idleDuration * 60 frames (idleDuration seconds).
              if (targetPosition.z >= cameraPosition.z)
              {
                  cameraPosition.z = cameraPosition.z + (Mathf.Abs(targetPosition.z - cameraPosition.z)+constMove) * Time.deltaTime * idleDuration;
              }

              if (targetPosition.z <= cameraPosition.z)
              {
                  cameraPosition.z = cameraPosition.z - (Mathf.Abs(targetPosition.z - cameraPosition.z)+constMove) * Time.deltaTime * idleDuration;
              }

              if (targetPosition.x >= cameraPosition.x)
              {
                  cameraPosition.x = cameraPosition.x + (Mathf.Abs(targetPosition.x - cameraPosition.x)+constMove) * Time.deltaTime * idleDuration;
              }

              if (targetPosition.x <= cameraPosition.x)
              {
                  cameraPosition.x = cameraPosition.x - (Mathf.Abs(targetPosition.x - cameraPosition.x)+constMove) * Time.deltaTime * idleDuration;
              }

              if (targetPosition.y >= cameraPosition.y)
              {
                  cameraPosition.y = cameraPosition.y + (Mathf.Abs(targetPosition.y - cameraPosition.y)+constMove) * Time.deltaTime * 0.5f * idleDuration;
              }

              if (targetPosition.y <= cameraPosition.y)
              {
                  cameraPosition.y = cameraPosition.y - (Mathf.Abs(targetPosition.y - cameraPosition.y)+constMove) * Time.deltaTime * 0.5f * idleDuration;
              }

            //}

            // This line of code allows the camera to smoothly jump ahead of the player in their direction at a high speed to
            // combat the code to return to the origin.
            Vector3 newPosition = cameraPosition + rotation.GetRotationDirection() * leadSpeed * Time.deltaTime;

            // This is the set of limits on the camera's lead, preventing it from going over leadMaxDistance in any direction.
            if (newPosition.z >= targetPosition.z + leadMaxDistance)
            {
                newPosition.z = targetPosition.z + leadMaxDistance;
            }
            if (newPosition.z <= targetPosition.z - leadMaxDistance)
            {
                newPosition.z = targetPosition.z - leadMaxDistance;
            }
            if (newPosition.x >= targetPosition.x + leadMaxDistance)
            {
                newPosition.x = targetPosition.x + leadMaxDistance;
            }
            if (newPosition.x <= targetPosition.x - leadMaxDistance)
            {
                newPosition.x = targetPosition.x - leadMaxDistance;
            }
            if (newPosition.y >= targetPosition.y + leadMaxDistance)
            {
                newPosition.y = targetPosition.y + leadMaxDistance;
            }
            if (newPosition.y <= targetPosition.y - leadMaxDistance)
            {
                newPosition.y = targetPosition.y - leadMaxDistance;
            }


            //targetPosition.z = 5000;
            this.managedCamera.transform.position = newPosition;


        }

    }

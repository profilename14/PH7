using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using System;

namespace KinematicCharacterController.Examples
{
    public class PlanetManager : MonoBehaviour, IMoverController
    {
        public PhysicsMover PlanetMover;
        public SphereCollider GravityField;
        public float GravityStrength = 10;
        public Vector3 OrbitAxis = Vector3.forward;
        public float OrbitSpeed = 10;

        public Teleporter OnPlaygroundTeleportingZone;
        public Teleporter OnPlanetTeleportingZone;

        private List<PlayerMovementController> _characterControllersOnPlanet = new List<PlayerMovementController>();
        private Vector3 _savedGravity;
        private Quaternion _lastRotation;

        private void Start()
        {
            OnPlaygroundTeleportingZone.OnCharacterTeleport -= ControlGravity;
            OnPlaygroundTeleportingZone.OnCharacterTeleport += ControlGravity;

            OnPlanetTeleportingZone.OnCharacterTeleport -= UnControlGravity;
            OnPlanetTeleportingZone.OnCharacterTeleport += UnControlGravity;

            _lastRotation = PlanetMover.transform.rotation;

            PlanetMover.MoverController = this;
        }

        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            goalPosition = PlanetMover.Rigidbody.position;

            // Rotate
            Quaternion targetRotation = Quaternion.Euler(OrbitAxis * OrbitSpeed * deltaTime) * _lastRotation;
            goalRotation = targetRotation;
            _lastRotation = targetRotation;

            // Apply gravity to characters
            foreach (PlayerMovementController cc in _characterControllersOnPlanet)
            {
                cc.gravity = (PlanetMover.transform.position - cc.transform.position).normalized * GravityStrength;
            }
        }

        void ControlGravity(PlayerMovementController cc)
        {
            _savedGravity = cc.gravity;
            _characterControllersOnPlanet.Add(cc);
        }

        void UnControlGravity(PlayerMovementController cc)
        {
            cc.gravity = _savedGravity;
            _characterControllersOnPlanet.Remove(cc);
        }
    }
}
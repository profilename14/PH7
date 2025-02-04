using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Animancer;
using UnityEditor;

public class EnemyMovementController : MonoBehaviour, ICharacterMovementController
{
    [SerializeField]
    Enemy enemy;

    [SerializeField]
    RichAI pathfinding;

    [SerializeField]
    protected Rigidbody _Rb;
    public Rigidbody rb => _Rb;

    [SerializeField]
    BaseCharacterData enemyData;

    protected bool velocityLocked = false;
    protected Vector3 internalLockedVelocity;

    protected bool rotationEnabled;

    protected bool movementEnabled;

    protected bool forceManualRotation;

    // GameObject created at runtime to visualize the Enemy's movement target.
    protected GameObject target;

    protected Vector3 groundNormal;

    [SerializeField]
    protected bool isGrounded;

    protected LayerMask groundMask;

    protected RaycastHit groundHit;

    protected bool disablePathfinding = false;

    protected EnemyActionManager actionManager;

    private void Awake()
    {
        pathfinding.maxSpeed = enemyData.maxBaseMoveSpeed;
        pathfinding.acceleration = enemyData.baseMoveAcceleration;
        pathfinding.rotationSpeed = enemyData.rotationSpeed;
        target = new GameObject(enemy.gameObject.name + " AI Target");
        var iconContent = EditorGUIUtility.IconContent("sv_label_1");
        EditorGUIUtility.SetIconForObject(target, (Texture2D)iconContent.image);
        groundMask = LayerMask.GetMask("Default", "Obstacles");
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (rotationEnabled && (!movementEnabled || forceManualRotation))
        {
            pathfinding.enableRotation = false;
            Vector3 targetDirection = target.transform.position - enemy.transform.position;
            float degrees = enemyData.rotationSpeed * Time.fixedDeltaTime;
            enemy.gameObject.transform.rotation = pathfinding.SimulateRotationTowards(targetDirection, degrees);
        }
        else
        {
            pathfinding.enableRotation = rotationEnabled;
        }

        if(Physics.Raycast(actionManager.gameObject.transform.position + 0.5f * Vector3.up, new Vector3(0, -1, 0), out groundHit, 1f, groundMask))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        //actionManager.gameObject.transform.localRotation = Quaternion.FromToRotation(actionManager.gameObject.transform.up, groundHit.normal);

        if(!disablePathfinding && isGrounded)
        {
            pathfinding.enabled = true;
        }
        else
        {
            //pathfinding.enabled = false;
        }
    }

    public bool ReachedDestination()
    {
        return pathfinding.reachedDestination;
    }

    public Vector3 GetPathfindingTargetPosition()
    {
        return target.transform.position;
    }

    public void SetForceManualRotation(bool isManualRotation)
    {
        forceManualRotation = isManualRotation;
    }

    public void SetPathfindingDestination(Vector3 position)
    {
        target.transform.position = position;
        pathfinding.destination = position;
    }

    public void SetAllowMovement(bool isAllowed)
    {
        pathfinding.isStopped = !isAllowed;
        movementEnabled = isAllowed;
    }

    public void SetAllowRotation(bool isAllowed)
    {
        rotationEnabled = isAllowed;
    }

    public void AddSpeedModifier(float modifier)
    {
        throw new System.NotImplementedException();
    }

    public void AddVelocity(Vector3 velocity)
    {
        rb.velocity += velocity;
    }

    public void ApplyImpulseForce(Vector3 direction, float power)
    {
        rb.AddForce(direction.normalized * power, ForceMode.Impulse);
    }

    public bool IsGrounded()
    {
        throw new System.NotImplementedException();
    }

    public void LockVelocity(Vector3 velocity)
    {
        internalLockedVelocity = velocity;
        velocityLocked = true;
    }

    public void RemoveSpeedModifier(float modifier)
    {
        throw new System.NotImplementedException();
    }

    public void SetVelocity(Vector3 velocity)
    {
        rb.velocity = velocity;
    }

    public void UnlockVelocity()
    {
        velocityLocked = false;
    }

    public void SetDrag(float drag)
    {
        rb.drag = drag;
    }

    public void SetAIEnabled(bool isEnabled)
    {
        disablePathfinding = !isEnabled;
    }

    void LateUpdate()
    {
        if (velocityLocked) rb.velocity = internalLockedVelocity;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref pathfinding);
        gameObject.GetComponentInParentOrChildren(ref _Rb);
        gameObject.GetComponentInParentOrChildren(ref enemy);
        gameObject.GetComponentInParentOrChildren(ref actionManager);
        enemyData = enemy.characterData;
    }
#endif
}

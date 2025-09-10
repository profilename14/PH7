using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Animancer;
using UnityEditor;

public class EnemyMovementController : CharacterMovementController
{
    [SerializeField]
    Enemy enemy;

    [SerializeField]
    public RichAI pathfinding;

    [SerializeField]
    public Seeker seeker;

    [SerializeField]
    public Rigidbody rb;

    [SerializeField]
    CharacterData enemyData;

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

    private float defaultDrag;

    private bool forceLookAtPlayer = false;

    private void Awake()
    {
        gameObject.GetComponentInParentOrChildren(ref pathfinding);
        gameObject.GetComponentInParentOrChildren(ref seeker);
        gameObject.GetComponentInParentOrChildren(ref rb);
        gameObject.GetComponentInParentOrChildren(ref enemy);
        gameObject.GetComponentInParentOrChildren(ref actionManager);
        enemyData = enemy.characterData;

        defaultDrag = rb.drag;

        pathfinding.maxSpeed = enemyData.maxBaseMoveSpeed;
        pathfinding.acceleration = enemyData.baseMoveAcceleration;
        pathfinding.rotationSpeed = enemyData.rotationSpeed;
        target = new GameObject(enemy.gameObject.name + " AI Target");

#if UNITY_EDITOR
        var iconContent = EditorGUIUtility.IconContent("sv_label_1");
        EditorGUIUtility.SetIconForObject(target, (Texture2D)iconContent.image);
#endif

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
            Vector3 targetDirection;
            float degrees;
            if (!forceLookAtPlayer)
            {
                targetDirection = target.transform.position - enemy.transform.position;
            }
            else
            {
                targetDirection = Player.instance.transform.position - enemy.transform.position;
            }

            degrees = enemyData.rotationSpeed * Time.fixedDeltaTime;

            enemy.gameObject.transform.rotation = pathfinding.SimulateRotationTowards(targetDirection, degrees);
        }
        else
        {
            pathfinding.enableRotation = rotationEnabled;
        }

        Debug.DrawRay(actionManager.gameObject.transform.position + 0.2f * Vector3.up, new Vector3(0, -1, 0));

        if(Physics.Raycast(actionManager.gameObject.transform.position + 0.2f * Vector3.up, new Vector3(0, -1, 0), out groundHit, 3f, groundMask))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
            rb.velocity += new Vector3(0, -0.4f, 0);
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

    public override void SetAllowMovement(bool isAllowed)
    {
        pathfinding.isStopped = !isAllowed;
        movementEnabled = isAllowed;
    }

    public override void SetAllowRotation(bool isAllowed)
    {
        rotationEnabled = isAllowed;
    }

    public void SetForceLookAtPlayer(bool lookAtPlayer)
    {
        forceLookAtPlayer = lookAtPlayer;
    }

    public override void AddSpeedModifier(float modifier)
    {
        throw new System.NotImplementedException();
    }

    public override Vector3 GetVelocity()
    {
        return pathfinding.velocity + rb.velocity;
    }

    public override void AddVelocity(Vector3 velocity)
    {
        rb.velocity += velocity;
    }

    public override void ApplyImpulseForce(Vector3 direction, float power)
    {
        rb.AddForce(direction.normalized * power, ForceMode.Impulse);
    }

    public override bool IsGrounded()
    {
        return isGrounded;
    }

    public override void LockVelocity(Vector3 velocity)
    {
        internalLockedVelocity = velocity;
        velocityLocked = true;
    }

    public override void RemoveSpeedModifier(float modifier)
    {
        throw new System.NotImplementedException();
    }

    public override void SetVelocity(Vector3 velocity)
    {
        rb.velocity = velocity;
    }

    public override void UnlockVelocity()
    {
        velocityLocked = false;
    }

    public override void SetGroundDrag(float drag)
    {
        rb.drag = drag;
    }

    public void ResetGroundDrag()
    {
        rb.drag = defaultDrag;
    }

    public void SetAIEnabled(bool isEnabled)
    {
        disablePathfinding = !isEnabled;
    }

    public void SetNewEnemyPath(Path p)
    {
        
    }

    void LateUpdate()
    {
        if (velocityLocked) rb.velocity = internalLockedVelocity;
    }

/*#if UNITY_EDITOR
    void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref pathfinding);
        gameObject.GetComponentInParentOrChildren(ref _Rb);
        gameObject.GetComponentInParentOrChildren(ref enemy);
        gameObject.GetComponentInParentOrChildren(ref actionManager);
        enemyData = enemy.characterData;
    }
#endif*/
}

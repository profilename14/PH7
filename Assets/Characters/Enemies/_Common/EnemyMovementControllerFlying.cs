using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using UnityEditor;

public class EnemyMovementControllerFlying : CharacterMovementController
{
    [SerializeField]
    Enemy enemy;

    [SerializeField]
    public Rigidbody rb;

    [SerializeField]
    CharacterData enemyData;

    protected bool velocityLocked = false;
    protected Vector3 internalLockedVelocity;

    protected bool rotationEnabled = true;

    protected bool movementEnabled = true;

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

    [SerializeField]
    private float maxXRotation;

    private void Awake()
    {
        gameObject.GetComponentInParentOrChildren(ref rb);
        gameObject.GetComponentInParentOrChildren(ref enemy);
        gameObject.GetComponentInParentOrChildren(ref actionManager);
        enemyData = enemy.characterData;

        defaultDrag = rb.drag;

        target = new GameObject(enemy.gameObject.name + " AI Target");

#if UNITY_EDITOR
        var iconContent = EditorGUIUtility.IconContent("sv_label_1");
        EditorGUIUtility.SetIconForObject(target, (Texture2D)iconContent.image);
#endif

        groundMask = LayerMask.GetMask("Default", "Obstacles");
    }

    private void FixedUpdate()
    {
        if (rotationEnabled)
        {
            //enemy.gameObject.transform.eulerAngles = new Vector3(Mathf.Clamp(enemy.gameObject.transform.eulerAngles.x, -maxXRotation, maxXRotation), enemy.gameObject.transform.eulerAngles.y, enemy.gameObject.transform.eulerAngles.z);

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

            Vector3 targetRotation = Quaternion.RotateTowards(enemy.gameObject.transform.rotation, Quaternion.LookRotation(targetDirection), degrees).eulerAngles;

            if (targetRotation.x > 180) targetRotation.x -= 360;
            targetRotation.x = Mathf.Clamp(targetRotation.x, -maxXRotation, maxXRotation);

            enemy.gameObject.transform.eulerAngles = targetRotation;
        }

        Debug.DrawRay(actionManager.gameObject.transform.position + 0.2f * Vector3.up, new Vector3(0, -1, 0));

        if (Physics.Raycast(actionManager.gameObject.transform.position + 0.2f * Vector3.up, new Vector3(0, -1, 0), out groundHit, 3f, groundMask))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        if(movementEnabled)
        {
            rb.AddForce((target.transform.position - enemy.gameObject.transform.position).normalized * enemyData.maxBaseMoveSpeed, ForceMode.Force);
        }
    }

    public void SetPathfindingDestination(Vector3 position)
    {
        target.transform.position = position;
    }

    public override void AddSpeedModifier(float modifier)
    {
        throw new System.NotImplementedException();
    }

    public override void AddVelocity(Vector3 velocity)
    {
        rb.velocity += velocity;
    }

    public override void ApplyImpulseForce(Vector3 direction, float power)
    {
        rb.AddForce(direction.normalized * power, ForceMode.Impulse);
    }

    public override Vector3 GetVelocity()
    {
        return rb.velocity;
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

    public override void SetAllowMovement(bool isAllowed)
    {
        movementEnabled = isAllowed;
    }

    public override void SetAllowRotation(bool isAllowed)
    {
        rotationEnabled = isAllowed;
    }

    public void SetForceLookRotation(bool isAllowed)
    {
        forceLookAtPlayer = isAllowed;
    }

    public override void SetGroundDrag(float drag)
    {
        rb.drag = drag;
    }

    public void ResetGroundDrag()
    {
        rb.drag = defaultDrag;
    }

    public override void SetVelocity(Vector3 velocity)
    {
        rb.velocity = velocity;
    }

    public override void UnlockVelocity()
    {
        velocityLocked = false;
    }

    void LateUpdate()
    {
        if (velocityLocked) rb.velocity = internalLockedVelocity;
    }
}

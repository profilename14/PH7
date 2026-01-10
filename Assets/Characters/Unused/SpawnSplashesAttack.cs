using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class SpawnSplashesAttack : AttackState
{
    [SerializeField]
    private ClipTransition summoningSplashes;

    [SerializeField]
    AoEProjectile[] projectilePool;

    [SerializeField]
    int numberOfSplashes;

    [SerializeField]
    float delay;

    private EnemyMovementController movementController;

    public override bool CanEnterState => _ActionManager.allowedStates[this];

    private Player player;

    private void Awake()
    {
        movementController = (EnemyMovementController)_Character.movementController;
    }

    private void Start()
    {
        player = Player.instance;
    }

    protected override void OnEnable()
    {
        _ActionManager.SetAllActionPriorityAllowed(false);
        movementController.SetAllowMovement(false);
        movementController.SetAllowRotation(true);

        _ActionManager.anim.Play(summoningSplashes);
    }

    protected IEnumerator SummonSplashes()
    {
        for(int i = 0; i < numberOfSplashes; i++)
        {
            yield return new WaitForSeconds(delay);
            foreach(MyProjectile p in projectilePool)
            {
                if (!p.projectileIsActive)
                {
                    p.InitProjectile(player.transform.position, Vector3.zero, _Character, attackData);
                };
            }
        }

        _ActionManager.StateMachine.ForceSetDefaultState();
    }
}

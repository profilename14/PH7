using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class CharacterLegAnimMixer : MonoBehaviour
{
    [SerializeField]
    Character character;

    [SerializeField]
    private TransitionAssetBase transition;

    [SerializeField]
    private AvatarMask legsMask;

    [SerializeField]
    private StringAsset moveVectorXName;
    [SerializeField]
    private StringAsset moveVectorYName;

    private AnimancerComponent anim;

    private AnimancerLayer legsLayer;

    private CharacterMovementController movementController;

    private Parameter<float> moveVectorX;
    private Parameter<float> moveVectorY;

    private void Awake()
    {
        anim = character.actionManager.anim;
        movementController = character.movementController;
        legsLayer = anim.Layers[1];
        moveVectorX = anim.Parameters.GetOrCreate<float>(moveVectorXName);
        moveVectorY = anim.Parameters.GetOrCreate<float>(moveVectorYName);
        legsLayer.Mask = legsMask;
        legsLayer.Play(transition);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 localVelocity = character.transform.InverseTransformVector(movementController.GetVelocity());
        moveVectorX.SetValue(localVelocity.x);
        moveVectorY.SetValue(localVelocity.z);
    }
}

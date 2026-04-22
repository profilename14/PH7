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

    [SerializeField]
    private StringAsset velocityName;

    private AnimancerComponent anim;

    private AnimancerLayer legsLayer;

    private CharacterMovementController movementController;

    private Parameter<float> moveVectorX;
    private Parameter<float> moveVectorY;
    private Parameter<float> velocity;

    private void Awake()
    {
        anim = character.actionManager.anim;
        movementController = character.movementController;
        legsLayer = anim.Layers[1];
        if(moveVectorXName) moveVectorX = anim.Parameters.GetOrCreate<float>(moveVectorXName);
        if(moveVectorYName) moveVectorY = anim.Parameters.GetOrCreate<float>(moveVectorYName);
        if(velocityName) velocity = anim.Parameters.GetOrCreate<float>(velocityName);
        legsLayer.Mask = legsMask;
        legsLayer.Play(transition);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 localVelocity = character.transform.InverseTransformVector(movementController.GetVelocity());
        if (moveVectorXName) moveVectorX.SetValue(localVelocity.x);
        if (moveVectorYName) moveVectorY.SetValue(localVelocity.z);
        if (velocityName) velocity.SetValue(localVelocity.magnitude);
    }

    public void StopMixer()
    {
        legsLayer.Stop();
    }

    public void ResumeMixer()
    {
        legsLayer.Play(transition);
    }
}

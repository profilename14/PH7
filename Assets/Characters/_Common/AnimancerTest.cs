using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class AnimancerTest : MonoBehaviour
{
    [SerializeField]
    private ClipTransition animationToPlay;

    private void Awake()
    {
        GetComponent<AnimancerComponent>().Play(animationToPlay);
    }
}

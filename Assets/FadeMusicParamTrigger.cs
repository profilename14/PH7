using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
using FMOD;

public class FadeMusicParamTrigger : MonoBehaviour
{
    private float musicIntensityParam;

    private bool hasTriggered = false;

    [SerializeField]
    bool isTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (!isTrigger) return;
        if (!other.gameObject.CompareTag("Player")) return;
        if (hasTriggered) return;

        hasTriggered = true;

        FadeIn();
    }

    public void FadeIn()
    {
        musicIntensityParam = 0;
        Tween.Custom(musicIntensityParam, endValue: 1, duration: 5, onValueChange: newVal => FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Intensity", newVal));
    }

    public void FadeOut()
    {
        musicIntensityParam = 1;
        Tween.Custom(musicIntensityParam, endValue: 0, duration: 5, onValueChange: newVal => FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Intensity", newVal));
    }
}

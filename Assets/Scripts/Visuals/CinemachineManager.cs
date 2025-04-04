using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.IO.LowLevel.Unsafe;

public class CinemachineManager : MonoBehaviour
{
    
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin perlinNoise;

    [SerializeField] float shakePower = 0.5f;
    [SerializeField] float shakeDuration = 0.5f;


    // Start is called before the first frame update
    void Start()
    {
        virtualCamera = gameObject.GetComponent<CinemachineVirtualCamera>();
        perlinNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlinNoise.m_AmplitudeGain = 0f;  // Reset shake

        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ScreenShake(float intensity, float duration)
    {
        StartCoroutine(ShakeRoutine(intensity, duration));
    }

    public void ScreenShake()
    {
        StartCoroutine(ShakeRoutine(shakePower, shakeDuration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        perlinNoise.m_AmplitudeGain = intensity;  // Set shake strength
        yield return new WaitForSeconds(duration);
        perlinNoise.m_AmplitudeGain = 0f;  // Reset shake
    }
}

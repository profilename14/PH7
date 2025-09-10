using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor.ShaderGraph.Internal;

public class CinemachineManager : MonoBehaviour
{

    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin perlinNoise;
    private CinemachineTransposer transposer;

    [SerializeField] float shakePower = 0.125f;
    [SerializeField] float shakeDuration = 0.15f;
    [SerializeField] float curAngle = 0;
    [SerializeField] float zoomOutMultiplier = 1;


    // Start is called before the first frame update
    void Start()
    {
        virtualCamera = gameObject.GetComponent<CinemachineVirtualCamera>();
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        perlinNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlinNoise.m_AmplitudeGain = 0f;  // Reset shake

    }

    // Update is called once per frame
    void Update()
    {

        rotateCamera(curAngle);
    }

    public void ScreenShake(float intensityMult, float durationMult)
    {
        StartCoroutine(ShakeRoutine(shakePower * intensityMult, shakeDuration * durationMult));
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

    public void rotateCamera(float angleDegrees)
    {

        transposer.m_FollowOffset = GetCameraPosition(angleDegrees, zoomOutMultiplier);

        virtualCamera.transform.rotation = GetCameraRotation(angleDegrees);

    }


    // angle = 0 for south (default), 90 for east, 180 for north, 270 for west
    // Set position to expected angle and rotation to expected angle to camera transition.
    public Vector3 GetCameraPosition(float angleDegrees = 0, float zoomOutMultiplier = 1, float radius = 28.28427f)
    {
        float height = 30;
        Vector3 center = new Vector3(0, 0, 0);

        float angleRad = (angleDegrees + 135) * Mathf.Deg2Rad;

        float x = center.x + radius * Mathf.Sin(angleRad);
        float z = center.z + radius * Mathf.Cos(angleRad);
        float y = height;

        return new Vector3(x, y, z) * zoomOutMultiplier;
    }

    // Pitch is the looking down angle, baseyaw is what changes as the camera orbits the player.
    public static Quaternion GetCameraRotation(float angleDegrees, float basePitch = 47f, float baseYaw = -48.3f)
    {
        float yaw = baseYaw + (angleDegrees);
        return Quaternion.Euler(basePitch, yaw, 0f);
    }
}

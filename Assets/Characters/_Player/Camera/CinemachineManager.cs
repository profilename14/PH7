using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.IO.LowLevel.Unsafe;

public class CinemachineManager : MonoBehaviour
{

    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin perlinNoise;
    private CinemachineTransposer transposer;

    const float baseYaw = -48.3f;
    const float basePitch = 47.3f;
    const float baseRoll = 0f;

    [SerializeField] float shakePower = 0.125f;
    [SerializeField] float shakeDuration = 0.15f;
    [SerializeField] float yaw = 0;
    [SerializeField] float pitch = 0;
    [SerializeField] float roll = 0;
    [SerializeField] float zoomOutMultiplier = 1;
    [SerializeField] Vector3 transposeVector = Vector3.zero;

    [SerializeField] float debugPanCameraSpeed = 10;


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

        rotateCameraYaw(yaw);

        rotateCameraPitch(pitch);

        /*if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            yaw -= 1 * Time.deltaTime;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            yaw += 1 * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            pitch -= 1 * Time.deltaTime;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            pitch += 1 * Time.deltaTime;
        }*/
    }

    public void ScreenShake(float intensityMult, float durationMult)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(shakePower * intensityMult, shakeDuration * durationMult));
    }

    public void ScreenShake()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(shakePower, shakeDuration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        perlinNoise.m_AmplitudeGain = intensity;  // Set shake strength
        yield return new WaitForSeconds(duration);
        perlinNoise.m_AmplitudeGain = 0f;  // Reset shake
    }

    public void rotateCameraYaw(float yaw)
    {

        transposer.m_FollowOffset = GetCameraPosition(yaw, pitch, zoomOutMultiplier);

        virtualCamera.transform.rotation = GetCameraRotation(yaw, pitch, roll);

    }

    public void rotateCameraPitch(float pitch)
    {

        transposer.m_FollowOffset = GetCameraPosition(yaw, pitch, zoomOutMultiplier);

        virtualCamera.transform.rotation = GetCameraRotation(yaw, pitch, roll);

    }

    public void DebugPanCamera(Vector2 rotation)
    {
        yaw += rotation.x * debugPanCameraSpeed;
        pitch -= rotation.y * debugPanCameraSpeed;
    }


    // angle = 0 for south (default), 90 for east, 180 for north, 270 for west of player
    // Set position to expected angle and rotation to expected angle to camera transition.
    public Vector3 GetCameraPosition(float yaw = 0, float pitch = 0, float zoomOutMultiplier = 1, float radius = 42.5f)
    {
        //float height = 30;
        Vector3 center = new Vector3(0, 0, 0);

        float angleRad = (yaw + (180 + baseYaw)) * Mathf.Deg2Rad;
        float pitchRad = (pitch + basePitch) * Mathf.Deg2Rad;

        float x = center.x + radius * Mathf.Sin(angleRad) * Mathf.Cos(pitchRad);
        float z = center.z + radius * Mathf.Cos(angleRad) * Mathf.Cos(pitchRad);
        float y = center.z + radius * Mathf.Sin(pitchRad);

        return (new Vector3(x, y, z) + transposeVector) * zoomOutMultiplier;
    }

    // Yaw is what changes as the camera orbits the player, Pitch is the looking up/down angle, roll is tilting the camera left/right.
    public static Quaternion GetCameraRotation(float yawOffset = 0f, float pitchOffset = 0f, float rollOffset = 0f)
    {
        float yaw = baseYaw + yawOffset;
        float pitch = basePitch + pitchOffset;
        float roll = baseRoll + rollOffset;
        return Quaternion.Euler(pitch, yaw, roll);
    }
}

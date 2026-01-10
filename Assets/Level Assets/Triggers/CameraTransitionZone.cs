using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class CameraTransitionZone : MonoBehaviour
{
    [SerializeField]
    CinemachineVirtualCamera virtualCamera;

    [SerializeField]
    private int priorityToSet = 11;
    private int originalPriority = 10;

    private void Awake()
    {
        if (virtualCamera == null) Destroy(gameObject);

        originalPriority = virtualCamera.Priority;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            virtualCamera.Priority = priorityToSet;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            virtualCamera.Priority = originalPriority;
        }
    }
}

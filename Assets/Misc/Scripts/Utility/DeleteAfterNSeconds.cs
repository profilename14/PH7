using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class DeleteAfterNSeconds : MonoBehaviour
{
    public float timeTillDeletion = 3;
    private float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > timeTillDeletion)
        {
            Destroy(gameObject);
        }
    }
}

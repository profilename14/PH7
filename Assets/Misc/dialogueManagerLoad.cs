using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dialogueManagerLoad : MonoBehaviour
{
    public static dialogueManagerLoad instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);

        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

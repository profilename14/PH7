using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class loadingFadeToBlack : MonoBehaviour
{
    public Image image;

    float fadeTimer = 0.5f;

    bool fading = false;


    // Start is called before the first frame update
    void Start()
    {
        image.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (fading && fadeTimer > 0.2f)
        {
            fadeTimer -= Time.deltaTime;
        }
        
    }

    public void fadeToBlackDoor()
    {
        
        image.enabled = true;
        SetAlpha(0.5f);
    }


    public void SetAlpha(float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class loadingFadeToBlack : MonoBehaviour
{
    public Image image;

    float fadeTimer = 0.5f;

    bool fadingFall = false;
    bool fadingFallPart2 = false;


    // Start is called before the first frame update
    void Start()
    {
        image.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(fadeTimer);
        if (fadingFall)
        {
            if (!fadingFallPart2)
            {
                fadeTimer += Time.deltaTime * 2;
                if (fadeTimer >= 1f)
                {
                    fadeTimer = 1f;
                    fadingFallPart2 = true;
                }
                SetAlpha(fadeTimer);
            }
            else
            {
                fadeTimer -= Time.deltaTime * 3;
                if (fadeTimer <= 0)
                {
                    fadeTimer = 0;
                }
                SetAlpha(fadeTimer);
                if (fadeTimer == 0)
                {
                    fadingFall = false;
                    fadingFallPart2 = false;
                    fadeTimer = 0.5f;
                    
                    image.enabled = false;
                }
            }

        }
        
    }

    public void fadeToBlackDoor()
    {
        
        image.enabled = true;
        SetAlpha(0.5f);
    }

    public void fadeToBlackFall()
    {
        fadingFall = true;
        image.enabled = true;
        //SetAlpha(0.5f);
    }

    public void SetAlpha(float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}

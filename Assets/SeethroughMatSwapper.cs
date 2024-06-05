using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeethroughMatSwapper : MonoBehaviour
{
    public Material defaultMaterial;
    public Material seethroughMaterial;

    public float fadeSpeed;

    public float fadePercent = 0.5f;

    public bool isSeethrough;

    private Material thisMaterial;

    private MeshRenderer thisRenderer;



    private void Start()
    {
        thisMaterial = GetComponent<MeshRenderer>().material;
        thisRenderer = GetComponent<MeshRenderer>();
    }

    public void StartFade()
    {
        isSeethrough = true;
        StopAllCoroutines();
        StartCoroutine(Fade());
    }

    public void StartReverseFade()
    {
        isSeethrough = false;
        StopAllCoroutines();
        StartCoroutine(ReverseFade());
    }

    public IEnumerator Fade()
    {
        thisRenderer.material = seethroughMaterial;
        thisMaterial = GetComponent<MeshRenderer>().material;
        Color currentColor = thisMaterial.color;

        //When current alpha is 1, t should be 0. When current alpha is 0.5, t should be 1.
        float t = -2*currentColor.a + 2;

        while(true)
        {
            Color smoothColor = new Color(currentColor.r, currentColor.g, currentColor.b,
                Mathf.Lerp(1, fadePercent, t));
            thisMaterial.color = smoothColor;

            t += fadeSpeed * Time.deltaTime;

            if (t >= 1) { break; }

            yield return null;
        }
    }

    public IEnumerator ReverseFade()
    {
        Color currentColor = thisMaterial.color;

        //When current alpha is 0.5, t should be 0. When current alpha is 1, t should be 1.
        float t = 2 * currentColor.a - 1;

        while (true)
        {
            Color smoothColor = new Color(currentColor.r, currentColor.g, currentColor.b,
                Mathf.Lerp(fadePercent, 1, t));
            thisMaterial.color = smoothColor;

            t += fadeSpeed * Time.deltaTime;

            if (t >= 1) { break; }

            yield return null;
        }

        thisRenderer.material = defaultMaterial;
    }
}

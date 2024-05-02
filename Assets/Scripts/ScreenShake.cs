using System.Collections;
using UnityEngine;

public class screenShake : MonoBehaviour
{
    public float duration = 1f;
    public AnimationCurve curve;
    public bool start = false;

    public void ScreenShake(float length)
    {
        start = true;
        duration = length;
    }

    private void Update()
    {
        if (start)
        {
            start = false;
            StartCoroutine(Shaking());
        }

    }

    IEnumerator Shaking()
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / duration);
            transform.position = startPosition + Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.position = startPosition;
    }
}
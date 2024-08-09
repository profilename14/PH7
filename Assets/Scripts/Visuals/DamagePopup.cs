using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro text;
    private float time;

    private void Awake()
    {
        time = 3f;
        text = transform.GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        time -= Time.deltaTime;

        if (time <= 0f)
        {
            Destroy(gameObject);
        }

        float ySpeed = 0.05f;
        transform.position += new Vector3(0, ySpeed, 0);
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }

    public void Setup(float damage)
    {
        text.SetText(damage.ToString());
    }
}

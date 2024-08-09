using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectUI : MonoBehaviour
{
    private ObjectWithPH obj;
    private Slider objPHBar;
    private float prevPH;

    private Vector3 originalScale;
    private bool isInvisible = true;

    public Transform camTransform;

    // Start is called before the first frame update
    void Start()
    {
        camTransform = GameObject.Find("Main Camera").transform;
        gameObject.GetComponent<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        obj = gameObject.transform.parent.gameObject.GetComponent<ObjectWithPH>();
        objPHBar = gameObject.transform.GetChild(0).gameObject.GetComponent<Slider>(); // Make sure the PH bar is after HP!

        objPHBar.maxValue = 14;
        prevPH = obj.CurrentPH;

        objPHBar.value = obj.CurrentPH;


        originalScale = transform.localScale;  // Make the UI invisible until an enemy is hit.
        transform.localScale = new Vector3(0, 0, 0);
        isInvisible = true;

    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(transform.position + camTransform.forward);
        // Kind of unoptimized vs running on the enemy damage function, but I'd like to avoid
        // giving the obj a reference to this UI (if it gets blanked out it'll be awful).
        if (prevPH != obj.CurrentPH) {
          objPHBar.value = obj.CurrentPH;
          prevPH = obj.CurrentPH;
          if (isInvisible == true) {
            isInvisible = false;
            transform.localScale = originalScale;
          }
        }

    }
}

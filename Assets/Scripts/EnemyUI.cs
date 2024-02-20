using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    private EnemyBehavior enemy;
    private Slider enemyHPBar;
    private Slider enemyPHBar;
    private float prevHealth;
    private float prevPH;

    private Vector3 originalScale;
    private bool isInvisible = true;

    public Transform camTransform;

    // Start is called before the first frame update
    void Start()
    {
        camTransform = GameObject.Find("Main Camera").transform;
        gameObject.GetComponent<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        enemy = gameObject.transform.parent.gameObject.GetComponent<EnemyBehavior>();
        enemyHPBar = gameObject.transform.GetChild(0).gameObject.GetComponent<Slider>();
        enemyPHBar = gameObject.transform.GetChild(1).gameObject.GetComponent<Slider>(); // Make sure the PH bar is after HP!
        enemyHPBar.maxValue = enemy.StartHealth;
        prevHealth = enemy.getHealth();
        enemyPHBar.maxValue = 14;
        prevPH = enemy.getCurPH();

        enemyPHBar.value = enemy.getCurPH();
        enemyHPBar.value = enemy.getHealth();


        originalScale = transform.localScale;  // Make the UI invisible until an enemy is hit.
        transform.localScale = new Vector3(0, 0, 0);
        isInvisible = true;

    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(transform.position + camTransform.forward);
        // Kind of unoptimized vs running on the enemy damage function, but I'd like to avoid
        // giving the enemy a reference to this UI (if it gets blanked out it'll be awful).
        if (prevHealth != enemy.getHealth()) {
          enemyHPBar.value = enemy.getHealth();
          prevHealth = enemy.getHealth();
          if (isInvisible == true) {
            isInvisible = false;
            transform.localScale = originalScale;
          }
        }
        /*if (prevPH != enemy.getCurPH()) {
          enemyPHBar.value = enemy.getCurPH();
          prevPH = enemy.getCurPH();
          if (isInvisible == true) {
            isInvisible = false;
            transform.localScale = originalScale;
          }
        }*/

    }
}

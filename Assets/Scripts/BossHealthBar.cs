using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    private Slider enemyHPBar;
    private float prevHealth;

    private Vector3 originalScale;
    private bool isInvisible = false;
    [SerializeField] bool hide = true;

    //public Transform camTransform;

    // Start is called before the first frame update
    void Start()
    {
        //camTransform = GameObject.Find("Main Camera").transform;
        //gameObject.GetComponentInParentOrChildren<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        enemyHPBar = gameObject.GetComponent<Slider>();
        enemyHPBar.maxValue = enemy.characterData.maxHealth;
        prevHealth = enemy.stats.health;

        enemyHPBar.value = enemy.stats.health;


        originalScale = transform.localScale;  // Make the UI invisible until an enemy is hit.
        if (hide) 
        {
          transform.localScale = new Vector3(0, 0, 0);
          isInvisible = true;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //transform.LookAt(transform.position + camTransform.forward);

        // Kind of unoptimized vs running on the enemy damage function, but I'd like to avoid
        // giving the enemy a reference to this UI (if it gets blanked out it'll be awful).
        if (prevHealth != enemy.stats.health) {
          enemyHPBar.value = enemy.stats.health;
          prevHealth = enemy.stats.health;
          if (isInvisible == true) {
            isInvisible = false;
            transform.localScale = originalScale;
          }
        }

    }
}

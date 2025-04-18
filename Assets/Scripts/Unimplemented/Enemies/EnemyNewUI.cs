using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyNewUI : MonoBehaviour
{
    private EnemyAI enemy;
    private Slider enemyHPBar;
    private Slider enemyArmorBar;
    private float prevHealth;
    private float prevArmor;

    private Vector3 originalScale;
    private bool isInvisible = true;
    private bool debuffed = false;
    [SerializeField] Sprite armorBarImage;
    [SerializeField] Sprite brokenArmorBarImage;
    [SerializeField] Image notDebuffedImage;
    [SerializeField] Image debuffedImage;
    private Image ArmorBarVisual;

    Transform camTransform;

    // Start is called before the first frame update
    void Start()
    {
        camTransform = GameObject.FindWithTag("MainCamera").transform;
        gameObject.GetComponent<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        enemy = gameObject.transform.parent.gameObject.GetComponent<EnemyAI>();
        enemyHPBar = gameObject.transform.GetChild(0).gameObject.GetComponent<Slider>();
        enemyArmorBar = gameObject.transform.GetChild(1).gameObject.GetComponent<Slider>(); // Make sure the Armor bar is after HP!
        enemyHPBar.maxValue = enemy.maxHealth;
        prevHealth = enemy.health;
        enemyArmorBar.maxValue = enemy.maxArmor;
        prevArmor = enemy.armor;

        enemyArmorBar.value = enemy.armor;
        enemyHPBar.value = enemy.health;

        ArmorBarVisual = enemyArmorBar.transform.GetChild(0).gameObject.GetComponent<Image>();
        
        notDebuffedImage.enabled = true;
        debuffedImage.enabled = false;


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
        if (prevHealth != enemy.health) {
          enemyHPBar.value = enemy.health;
          prevHealth = enemy.health;
          if (isInvisible == true) {
            isInvisible = false;
            transform.localScale = originalScale;
          }
        }
        if (prevArmor != enemy.armor) {
          enemyArmorBar.value = enemy.armor;
          prevArmor = enemy.armor;
          if (isInvisible == true) {
            isInvisible = false;
            transform.localScale = originalScale;
          }
        }

        if (enemy.debuffTimer > 0 && debuffed == false) {
          ArmorBarVisual.sprite = brokenArmorBarImage;
          notDebuffedImage.enabled = false;
          debuffedImage.enabled = true;
          debuffed = true;
        } else if (enemy.debuffTimer <= 0 && debuffed == true) {
          ArmorBarVisual.sprite = armorBarImage;
          notDebuffedImage.enabled = true;
          debuffedImage.enabled = false;
          debuffed = false;
        }


    }
}

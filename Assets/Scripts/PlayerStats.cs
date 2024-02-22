using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public float health = 100;
    public float ph = 14;

    const float HEALTH_MAX = 100;
    const float PH_DEFAULT = 14;

    public float healthRegen = 3;
    public float phRegen = 0.1f;

    public Slider healthBar;
    public Slider PHBar;
    private PlayerCombatController combatController;

    [HideInInspector] public bool dyingState = false;
    private float deathRate = 14f; // 100/14 = about 7 seconds
    [HideInInspector] public bool rainPower = false; // If the player has a rain attack speed boost
    [HideInInspector] public bool hydroxidePower = false; // If the player just went over 14 with hydroxide drain
    [HideInInspector] public bool strongBaseMode = false; // If the playe is in their super mode from the above ^
    private float baseModeNeutralizeSpeed = 1.0f; // Lose 1 ph per second, can be extended with hydroxide drains

    Material NormalSigilMaterial;
    [SerializeField] Material StrongBaseSigilMaterial; // Mask glows in strong base mode
    [SerializeField] SkinnedMeshRenderer Mask;
    // int lives = 3; // Resets on the nexxt level because its a different typhis prefab


    // Start is called before the first frame update
    void Start()
    {
      healthBar = GameObject.FindWithTag("Health Bar").GetComponent<Slider>();
      PHBar = GameObject.FindWithTag("PH Bar").GetComponent<Slider>();

      healthBar.maxValue= HEALTH_MAX;

      Material[] NumMat;
      NumMat = Mask.materials;
      NormalSigilMaterial = NumMat[1];

      combatController = GetComponentInChildren<PlayerCombatController>();
    }

    // Update is called once per frame
    void Update()
    {
      if (ph < PH_DEFAULT) {
        ph += phRegen * Time.deltaTime;
      } else if (ph > PH_DEFAULT) {
        ph = PH_DEFAULT;
      }

      if (health < HEALTH_MAX && !dyingState) {
        health += healthRegen * Time.deltaTime;
      } else if (health > HEALTH_MAX) {
        health = HEALTH_MAX;
      }

      if (hydroxidePower && ! strongBaseMode) {
        strongBaseMode = true;
        hydroxidePower = false;
        // Other stuff to buff attack speed
        Material[] NumMat;
        NumMat = Mask.materials;
        NumMat[1] = StrongBaseSigilMaterial;
        Mask.materials = NumMat;

      }
      if (strongBaseMode) {
        ph -= Time.deltaTime * baseModeNeutralizeSpeed;
        if (ph <= 7) {
          strongBaseMode = false;
          // disable strong base mode
          Material[] NumMat;
          NumMat = Mask.materials;
          NumMat[1] = NormalSigilMaterial;
          Mask.materials = NumMat;
        }
      }

      if (ph <= 0 && ! dyingState) {
        dyingState = true;
        combatController.hydroxideCastTimer = 0;
        ph = -2; // Player has to ph drain or find an alkaline puddle to get back to zero.
        healthBar.gameObject.transform.localScale = new Vector3(1, 1, 1);
      }

      if (dyingState) {
        health -= Time.deltaTime * deathRate;
        if (health <= 0) {
          Destroy(gameObject); // No camera is displaying appears, but hey at least it stops gameplay
        }
        else if (ph > 0) {
          dyingState = false;
          healthBar.gameObject.transform.localScale = new Vector3(0, 0, 0);
        }
      }


      healthBar.value= health;
      PHBar.value = 16 + 80 * (ph / PH_DEFAULT);
    }

    public void playerDamage(float damage, float phChange, Vector3 position, float knockback) {
      bool isPlayerDashing = gameObject.GetComponent<MovementController>().isDashing;
      if (isPlayerDashing) {
        return;
      }

      ph -= phChange;

      if (ph > PH_DEFAULT) {
        ph = PH_DEFAULT;
      } else if (ph < 0) {
        ph = -1;
      }

      // Health got turned into a timer lol

      //float pHDifference = Mathf.Abs(PH_DEFAULT - ph);
      //float multiplier = 1 + 0.057f * Mathf.Pow(pHDifference, 1.496f);
      //health -= damage * multiplier;

      if (ph <= 0) {
        dyingState = true;
        // lives--;
        // if (lives < 0) {
        //   Destroy(gameObject);
        // }
        ph = -1; // Player has to ph drain or find an alkaline puddle to get back to zero.
        healthBar.gameObject.transform.localScale = new Vector3(1, 1, 1);
      }

      gameObject.GetComponent<MovementController>().applyKnockback(position, knockback);

    }
}

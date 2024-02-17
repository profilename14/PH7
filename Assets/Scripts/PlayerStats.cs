using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public float health = 100;

    const float HEALTH_MAX = 100;
    
    [SerializeField] public PHSubstance phSubstance;

    public float healthRegen = 3;

    public Slider healthBar;
    public Slider PHBar;

    // Start is called before the first frame update
    void Start()
    {
      healthBar = GameObject.FindWithTag("Health Bar").GetComponent<Slider>();
      PHBar = GameObject.FindWithTag("PH Bar").GetComponent<Slider>();

      healthBar.maxValue= HEALTH_MAX;
    }

    // Update is called once per frame
    void Update()
    {
      phSubstance.Naturalize(Time.deltaTime);

      if (health < HEALTH_MAX) {
        health += healthRegen * Time.deltaTime;
      } else if (health > HEALTH_MAX) {
        health = HEALTH_MAX;
      }

      healthBar.value= health;
      PHBar.value = phSubstance.pHBarValue();
    }

    public void playerDamage(float damage, float attackPH, float attackVol, Vector3 position, float knockback) {
      bool isPlayerDashing = gameObject.GetComponent<MovementController>().isDashing;
      if (isPlayerDashing) {
        return;
      }
      
      // Take damage first, then change pH

      float pHDifference = Mathf.Abs(phSubstance.naturalPH - phSubstance.GetPh());
      float multiplier = 1 + 0.057f * Mathf.Pow(pHDifference, 1.496f);
      health -= damage * multiplier;

      phSubstance.MixWith(attackPH, attackVol);
      
      if (health < 0) {
        Destroy(gameObject); // No camera is displaying appears, but hey at least it stops gameplay
      }
      
      gameObject.GetComponent<MovementController>().applyKnockback(position, knockback);
    }
}

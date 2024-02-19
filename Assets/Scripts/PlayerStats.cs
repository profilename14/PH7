using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public float health = 100;
    public float ph = 7;

    const float HEALTH_MAX = 100;
    const float PH_DEFAULT = 7;

    public float healthRegen = 3;
    public float phRegen = 0.33f;

    public Slider healthBar;
    public Slider PHBar;

    // Start is called before the first frame update
    void Start()
    {
        healthBar = GameObject.FindWithTag("Health Bar").GetComponent<Slider>();
        PHBar = GameObject.FindWithTag("PH Bar").GetComponent<Slider>();

        healthBar.maxValue = HEALTH_MAX;
        PHBar.value = 7;
    }

    // Update is called once per frame
    void Update()
    {
        if (ph < PH_DEFAULT) {
            ph += phRegen * Time.deltaTime;
            if (ph > PH_DEFAULT) ph = PH_DEFAULT;
        }
        else if (ph > PH_DEFAULT) {
            ph -= phRegen * Time.deltaTime;
            if (ph < PH_DEFAULT) ph = PH_DEFAULT;
        }

        if (health < HEALTH_MAX) {
            health += healthRegen * Time.deltaTime;
        } else if (health > HEALTH_MAX) {
            health = HEALTH_MAX;
        }

        healthBar.value = health;
        PHBar.value = 16 + 80 * (ph / 14);
    }

    public void playerDamage(float damage, float attackPH, float phChange, Vector3 position, float knockback) {
        bool isPlayerDashing = gameObject.GetComponent<MovementController>().isDashing;
        if (isPlayerDashing) {
            return;
        }

        ph += phChange;

        if (ph > 14) {
            ph = 14;
        } else if (ph < 0) {
            ph = 0;
        }

        /*float pHDifference = Mathf.Abs(PH_DEFAULT - ph);
        float multiplier = 1 + 0.057f * Mathf.Pow(pHDifference, 1.496f);
        health -= damage * multiplier;*/
        
        //Defensive damage multiplier. Maximum of ~3.5x.
        float multiplier = 1;

        if(attackPH < 7 && ph < 7)
        {
            multiplier = Mathf.Pow(1.3f, (ph - 7));
        }
        else if(attackPH > 7 && ph > 7)
        {
            multiplier = Mathf.Pow(1.3f, (7 - ph));
        }

        Debug.Log("Player took damage: " + damage * multiplier + " with a multiplier of " + multiplier);

        health -= damage * multiplier;

        if (health < 0) {
            Destroy(gameObject); // No camera is displaying appears, but hey at least it stops gameplay
        }

        gameObject.GetComponent<MovementController>().applyKnockback(position, knockback);

    }
}

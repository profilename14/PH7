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
    public float phRegen = 0.33f;

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
      if (ph < PH_DEFAULT) {
        ph += phRegen * Time.deltaTime;
      } else if (ph > PH_DEFAULT) {
        ph = PH_DEFAULT;
      }

      if (health < HEALTH_MAX) {
        health += healthRegen * Time.deltaTime;
      } else if (health > HEALTH_MAX) {
        health = HEALTH_MAX;
      }

      healthBar.value= health;
      PHBar.value = 16 + 80 * (ph / PH_DEFAULT);
    }
}

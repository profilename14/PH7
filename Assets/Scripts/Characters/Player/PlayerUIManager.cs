using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    const float PH_DEFAULT = 14;

    [SerializeField] private PlayerStats playerStats;

    private Slider healthBar;
    private Slider PHBar;
    private Slider PHBar2;
    private Slider AcidBar;
    private Slider AcidBar2;
    public Image AlkalineIndicator;
    public Image AcidIndicator;

    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(this.gameObject);
      healthBar = GameObject.FindWithTag("Health Bar").GetComponent<Slider>();
      GameObject PHUI = GameObject.FindWithTag("PH Bar");
      PHBar = PHUI.transform.GetChild(0).GetComponent<Slider>();
      PHBar2 = PHUI.transform.GetChild(1).GetComponent<Slider>();
      AcidBar = PHUI.transform.GetChild(2).GetComponent<Slider>();
      AcidBar2 = PHUI.transform.GetChild(3).GetComponent<Slider>();

      healthBar.maxValue= playerStats.healthMax;

    }

    // Update is called once per frame
    void Update()
    {


      healthBar.value = playerStats.health;
      PHBar.value = 4 + 80 * (playerStats.alkaline / PH_DEFAULT);
      AcidBar.value = 16 + 80 * (playerStats.acid / PH_DEFAULT);

      
      if (playerStats.alkaline > 7) {
        AlkalineIndicator.enabled = true;
        PHBar.value = 7;
        PHBar2.value = (playerStats.alkaline-7) / 7;
      } else {
        AlkalineIndicator.enabled = false;
        PHBar.value = ((playerStats.alkaline) / 7);
        PHBar2.value = 0;

      }

      if (playerStats.acid > 7) {
        AcidIndicator.enabled = true;
        AcidBar.value = 7;
        AcidBar2.value = (playerStats.acid-7) / 7;
      } else {
        AcidIndicator.enabled = false;
        AcidBar.value = ((playerStats.acid) / 7);
        AcidBar2.value = 0;

      }


    
    }
}
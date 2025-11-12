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

    private float previousAlkaline = 0;

    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(this.gameObject);
      healthBar = GameObject.FindWithTag("Health Bar").GetComponent<Slider>();
      GameObject PHUI = GameObject.FindWithTag("PH Bar");
      AcidBar = PHUI.transform.GetChild(0).GetComponent<Slider>();
      AcidBar2 = PHUI.transform.GetChild(1).GetComponent<Slider>();
      PHBar = PHUI.transform.GetChild(2).GetComponent<Slider>();
      PHBar2 = PHUI.transform.GetChild(3).GetComponent<Slider>();

      healthBar.maxValue = playerStats.healthMax;
      previousAlkaline = (float)playerStats.alkaline;

    }

  // Update is called once per frame
  void FixedUpdate()
  {


    healthBar.value = playerStats.health;
    //PHBar.value = 4 + 80 * ((float)playerStats.alkaline / PH_DEFAULT);
    //AcidBar.value = 16 + 80 * ((float)playerStats.acid / PH_DEFAULT);

    //Debug.Log("alkaline" + (float)playerStats.alkaline);
    //Debug.Log("acid" + (float)playerStats.acid);

    if (previousAlkaline == (float)playerStats.alkaline)
    {
      return;
    }
    else
    {
      previousAlkaline = (float)playerStats.alkaline;
    }

    if (playerStats.alkaline >= 5.8)
    {
      AlkalineIndicator.enabled = true;
      //PHBar.value = 7;
      PHBar2.value = ((float)playerStats.alkaline) / 10;
    }
    else
    {
      AlkalineIndicator.enabled = false;
      PHBar2.value = ((float)playerStats.alkaline) / 10;
      //PHBar.value = (((float)playerStats.alkaline) / 7);
      //PHBar2.value = 0;
    }

        /*if (playerStats.alkaline > 7) {
          AlkalineIndicator.enabled = true;
          PHBar.value = 7;
          PHBar2.value = ((float)playerStats.alkaline-7) / 7;
        } else {
          AlkalineIndicator.enabled = false;
          PHBar.value = (((float)playerStats.alkaline) / 7);
          PHBar2.value = 0;

        }*/

        /*if (playerStats.acid > 7) {
          AcidIndicator.enabled = true;
          AcidBar.value = 7;
          AcidBar2.value = ((float)playerStats.acid-7) / 7;
        } else {
          AcidIndicator.enabled = false;
          AcidBar.value = (((float)playerStats.acid) / 7);
          AcidBar2.value = 0;

        }*/



    }
}
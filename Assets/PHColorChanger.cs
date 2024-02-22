using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PHColorChanger : MonoBehaviour
{
    private PlayerStats playerStats;

    private EnemyBehavior enemyStats;

    private bool isPlayer;

    private bool isAlkaline;

    [SerializeField]
    private PHGradients phGradients;

    [SerializeField]
    private GameObject mainColorObject;

    [SerializeField]
    private GameObject secondColorObject;

    [SerializeField]
    private GameObject thirdColorObject;

    [SerializeField]
    private GameObject fourthColorObject;

    private SkinnedMeshRenderer mainRenderer;
    private SkinnedMeshRenderer secondRenderer;
    private SkinnedMeshRenderer thirdRenderer;
    private SkinnedMeshRenderer fourthRenderer;

    [SerializeField]
    private Material mainMaterial;
    [SerializeField]
    private Material secondMaterial;
    [SerializeField]
    private Material thirdMaterial;
    [SerializeField]
    private Material fourthMaterial;

    private int mainMaterialIndex;
    private int secondMaterialIndex;
    private int thirdMaterialIndex;
    private int fourthMaterialIndex;

    // Start is called before the first frame update
    void Awake()
    {
        mainRenderer = mainColorObject.GetComponent<SkinnedMeshRenderer>();

        if (this.gameObject.CompareTag("Player"))
        {
            secondRenderer = secondColorObject.GetComponent<SkinnedMeshRenderer>();
            thirdRenderer = thirdColorObject.GetComponent<SkinnedMeshRenderer>();
            fourthRenderer = fourthColorObject.GetComponent<SkinnedMeshRenderer>();

            playerStats = GetComponent<PlayerStats>();
            isPlayer = true;

            int j = 0;
            foreach (Material m in secondRenderer.materials)
            {
                if (m.name == secondMaterial.name + " (Instance)")
                {
                    secondMaterialIndex = j;
                    break;
                }
                j++;
            }

            int k = 0;
            foreach (Material m in thirdRenderer.materials)
            {
                if (m.name == thirdMaterial.name + " (Instance)")
                {
                    thirdMaterialIndex = k;
                    break;
                }
                k++;
            }

            int l = 0;
            foreach (Material m in fourthRenderer.materials)
            {
                if (m.name == fourthMaterial.name + " (Instance)")
                {
                    fourthMaterialIndex = l;
                    break;
                }
                l++;
            }
        }
        else if(this.gameObject.CompareTag("Enemy"))
        {
            enemyStats = GetComponent<EnemyBehavior>();

            if (enemyStats.phDefaultType == PHDefaultType.Alkaline) isAlkaline = true;
            else isAlkaline = false;

            isPlayer = false;
        }

        int i = 0;
        foreach (Material m in mainRenderer.materials)
        {
            if (m.name == mainMaterial.name + " (Instance)")
            {
                mainMaterialIndex = i;
                break;
            }
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(isPlayer)
        {
            mainRenderer.materials[mainMaterialIndex].color = phGradients.typhisBodyPHGradient.Evaluate(playerStats.ph / 14);
            secondRenderer.materials[secondMaterialIndex].color = phGradients.typhisMaskPHGradient.Evaluate(playerStats.ph / 14);
            thirdRenderer.materials[thirdMaterialIndex].color = phGradients.typhisAlgaePHGradient.Evaluate(playerStats.ph / 14);
            fourthRenderer.materials[fourthMaterialIndex].color = phGradients.typhisStrandsPHGradient.Evaluate(playerStats.ph / 14);
        }
        else
        {
            //Debug.Log(this.gameObject.name + " ph on gradient is " + enemyStats.CurrentPH / 14);
            if(isAlkaline) mainRenderer.materials[mainMaterialIndex].color = phGradients.alkalinePHGradient.Evaluate((enemyStats.CurrentPH - 7) / 7);
            else mainRenderer.materials[mainMaterialIndex].color = phGradients.acidicPHGradient.Evaluate(enemyStats.CurrentPH / 7);
        }
    }
}

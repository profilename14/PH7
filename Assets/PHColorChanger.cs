using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PHColorChanger : MonoBehaviour
{
    private PlayerStats playerStats;

    private EnemyBehavior enemyStats;

    private bool isPlayer;

    [SerializeField]
    private PHGradients phGradients;

    [SerializeField]
    private GameObject mainColorObject;

    private SkinnedMeshRenderer mainRenderer;

    [SerializeField]
    private Material mainMaterial;

    private int mainMaterialIndex;

    // Start is called before the first frame update
    void Awake()
    {
        mainRenderer = mainColorObject.GetComponent<SkinnedMeshRenderer>();

        if(this.gameObject.CompareTag("Player"))
        {
            playerStats = GetComponent<PlayerStats>();
            isPlayer = true;
        }
        else if(this.gameObject.CompareTag("Enemy"))
        {
            enemyStats = GetComponent<EnemyBehavior>();
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
            mainRenderer.materials[mainMaterialIndex].color = phGradients.mainPHGradient.Evaluate(playerStats.ph / 14);
        }
        else
        {
            //Debug.Log(this.gameObject.name + " ph on gradient is " + enemyStats.CurrentPH / 14);
            mainRenderer.materials[mainMaterialIndex].color = phGradients.mainPHGradient.Evaluate(enemyStats.CurrentPH / 14);
        }
    }
}

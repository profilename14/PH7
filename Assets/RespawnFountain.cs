using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnFountain : MonoBehaviour
{
    public Transform typhisRespawnPosition;
    //public GameObject tooltip;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Player.instance.playerActionManager.interactCallback += UseFountain;
            //tooltip.SetActive(true);
            UseFountain();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Player.instance.playerActionManager.interactCallback -= UseFountain;
            //tooltip.SetActive(false);
        }
    }

    private void UseFountain()
    {
        GameManager.instance.respawnPosition = typhisRespawnPosition.position;
        Player.instance.stats.SetHealth(Player.instance.characterData.maxHealth);
        GameManager.instance.sceneToLoadOnRespawn = SceneManager.GetActiveScene().name;
        StartCoroutine(fountainVFX());
    }

    private IEnumerator fountainVFX()
    {
        Player.instance.playerVFXManager.FullyChargedVFX();

        yield return new WaitForSeconds(0.8f);

        Player.instance.playerVFXManager.EndChargeVFX();
        
    }
}

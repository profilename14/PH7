using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarStriderAnimEventRelay : MonoBehaviour
{
    public WarStriderAI aiScript;

    public void PauseStartupForSeconds(float seconds)
    {
        aiScript.PauseStartupForSeconds(seconds);
    }

    public void StopHitstun()
    {
        aiScript.StopHitstun();
    }

    public void StartAttack()
    {
        aiScript.StartAttack();
    }

    public void StartupFinished()
    {
        aiScript.StartupFinished();
    }

    public void DashStop()
    {
        aiScript.DashStop();
    }
    
    public void EndAttack()
    {
        aiScript.EndAttack();
    }

    public void FinishedFalling()
    {
        aiScript.FinishedFalling();
    }

    public void FinishedTakingOff()
    {
        aiScript.FinishedFalling();
    }

    public void Die()
    {
        Debug.Log("Die");
        aiScript.Die();
    }
}

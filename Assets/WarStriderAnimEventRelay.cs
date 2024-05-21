using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarStriderAnimEventRelay : MonoBehaviour
{
    public StriderAI aiScript;

    public void PauseStartupForSeconds(float seconds)
    {
        aiScript.PauseStartupForSeconds(seconds);
    }

    public void StopHitstun()
    {
        aiScript.StopHitstun();
    }

    public void StartAttack(string state)
    {
        aiScript.StartAttack(state);
    }

    public void DashStop()
    {
        aiScript.DashStop();
    }

    public void EndAttack()
    {
        aiScript.EndAttack();
    }

    public void Die()
    {
        Debug.Log("Die");
        aiScript.Die();
    }

    public void FinishedFalling()
    {

    }

    public void FinishedTakingOff()
    {

    }
}

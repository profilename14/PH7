using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScuttlerAnimEventRelay : MonoBehaviour
{
    public ScuttlerAI aiScript;

    public void PauseStartupForSeconds(float seconds)
    {
        aiScript.PauseStartupForSeconds(seconds);
    }

    public void StopHitstun()
    {
        aiScript.StopHitstun();
    }

    public void StartupFinished()
    {
        aiScript.StartupFinished();
    }

    public void StartAttack(string state)
    {
        aiScript.StartAttack(state);
    }

    public void EndAttack()
    {
        aiScript.EndAttack();
    }

    public void Die()
    {
        aiScript.Die();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StriderAnimEventRelay : MonoBehaviour
{
    public StriderAI aiScript;

    public void PauseStartupForSeconds(float seconds)
    {
        aiScript.PauseStartupForSeconds(seconds);
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
}

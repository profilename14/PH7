using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VitriclawAnimEventRelay : MonoBehaviour
{
    public VitriclawAI aiScript;

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

    public void JumpPeak()
    {
        aiScript.JumpPeak();
    }

    public void LandJump()
    {
        aiScript.LandJump();
    }
    
    public void EndAttack()
    {
        aiScript.EndAttack();
    }
}

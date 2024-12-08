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

    public void StopHitstun()
    {
        aiScript.StopHitstun();
    }

    public void StartupFinished()
    {
        aiScript.StartupFinished();
    }

    public void StartAttack()
    {
        aiScript.StartAttack();
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

    public void Die()
    {
        aiScript.Die();
    }
}

using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Unity.VisualScripting;
using UnityEngine;

public static class GameManager
{
    public static bool isControllerUsed = false;
    public static bool isScreenshakeEnabled = true;
    // Start is called before the first frame update
    public static float slowTimer = 0.0f;

    public static void slowdownTime(float rate, float length) {
        Time.timeScale = rate;
        slowTimer = length;
    }
}

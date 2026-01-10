using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public static class GameManagerOLD
{
    public static bool isControllerUsed = false;
    public static bool isMusicPlaying = false;
    public static bool isScreenshakeEnabled = true;
    // Start is called before the first frame update
    public static float slowTimer = 0.0f;

    public static void slowdownTime(float rate, float length) {
        Time.timeScale = rate;
        slowTimer = length;
    }
}

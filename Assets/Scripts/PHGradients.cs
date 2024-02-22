using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PHGradients", order = 1)]

public class PHGradients : ScriptableObject
{
    public Gradient typhisBodyPHGradient;
    public Gradient typhisMaskPHGradient;
    public Gradient typhisAlgaePHGradient;
    public Gradient typhisStrandsPHGradient;
    public Gradient alkalinePHGradient;
    public Gradient acidicPHGradient;
}

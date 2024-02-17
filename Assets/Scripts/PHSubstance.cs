using System;
using UnityEngine;

[Serializable] public class PHSubstance
{
    [SerializeField] public float pH = 7;
    [SerializeField]  public float naturalizationVol = 0.33f; // Volume of the substance of natural pH that is added per second
    public readonly float naturalPH = 7; // pH at which the substance wants to be in

    private readonly float volume = 100; // Resistance to pH change

    public PHSubstance(float naturalPH, float volume)
    {
        this.naturalPH = naturalPH;
        this.pH = naturalPH;
        this.volume = volume;
    }
    
    public float GetPh()
    {
        return pH;
    }

    public float pHBarValue()
    {
        return 16 + 80 * (pH / naturalPH);
    }
    
    public void Naturalize(float deltaTime)
    {
        MixWith(naturalPH, naturalizationVol * deltaTime);
    }

    public void MixWith(float addPH, float addVolume)
    {
        var hPlusConcentration = (float)Math.Pow(10, -pH);
        var addHPlusConcentration = (float)Math.Pow(10, -addPH);
        
        var newHPlusConcentration = (hPlusConcentration * volume + addHPlusConcentration * addVolume) / (volume + addVolume);
        pH = -(float)Math.Log10(newHPlusConcentration);
        
        // don't let pH go below 0 or above 14
        pH = Math.Max(0, Math.Min(14, pH));
    }
}
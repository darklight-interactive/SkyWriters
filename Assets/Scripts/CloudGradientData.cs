using UnityEngine;

[System.Serializable]
public class CloudGradientData
{
    public Color startColor = Color.white;
    public Color middleColor = Color.white;
    public Color endColor = Color.white;

    public CloudGradientData(Color startColor, Color middleColor, Color endColor)
    {
        this.startColor = startColor;
        this.middleColor = middleColor;
        this.endColor = endColor;
    }

    public Gradient ToGradient()
    {
        Gradient gradient = new Gradient();

        GradientColorKey[] colorKeys = new GradientColorKey[3]
        {
            new GradientColorKey(startColor, 0.0f),
            new GradientColorKey(middleColor, 0.5f),
            new GradientColorKey(endColor, 1.0f)
        };

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3]
        {
            new GradientAlphaKey(startColor.a, 0.0f),
            new GradientAlphaKey(middleColor.a, 0.5f),
            new GradientAlphaKey(endColor.a, 1.0f)
        };

        gradient.SetKeys(colorKeys, alphaKeys);

        return gradient;
    }
}
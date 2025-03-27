using UnityEngine;
using TMPro;

public class ProductionRatePanel : MonoBehaviour
{
    public PlayerResourceSystem playerSystem;

    public TextMeshProUGUI tubesText;
    public TextMeshProUGUI wiringText;
    public TextMeshProUGUI circuitsText;

    void Update()
    {
        playerSystem.GetRequiredRates(out float reqTubes, out float reqWiring, out float reqCircuits);

        float prodTubes = playerSystem.GetTubeRate();
        float prodWiring = playerSystem.GetWiringRate();
        float prodCircuits = playerSystem.GetCircuitRate();

        UpdateText(tubesText, prodTubes, reqTubes, "Tubes");
        UpdateText(wiringText, prodWiring, reqWiring, "Wiring");
        UpdateText(circuitsText, prodCircuits, reqCircuits, "Circuits");
    }

    void UpdateText(TextMeshProUGUI text, float current, float required, string label)
    {
        Color color;
        float ratio = (required > 0) ? current / required : 1f;

        if (ratio >= 1f)
            color = Color.green;
        else if (ratio >= 0.8f)
            color = Color.yellow;
        else
            color = Color.red;

        text.color = color;
        text.text = $"{label}: {current:F2} / {required:F2} /s";
    }
}

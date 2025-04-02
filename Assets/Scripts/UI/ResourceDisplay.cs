using UnityEngine;
using TMPro;

public class ResourceDisplay : MonoBehaviour
{
    public PlayerResourceSystem playerSystem;

    public TextMeshProUGUI ironText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI copperText;
    public TextMeshProUGUI siliconText;

    public TextMeshProUGUI metalTubesText;
    public TextMeshProUGUI wiringText;
    public TextMeshProUGUI circuitsText;

    void Update()
    {
        if (playerSystem == null) return;

        ironText.text = $"Iron: {playerSystem.iron:F0}";
        energyText.text = $"Energy: {playerSystem.energy:F0}";
        copperText.text = $"Copper: {playerSystem.copper:F0}";
        siliconText.text = $"Silicon: {playerSystem.silicon:F0}";

        metalTubesText.text = $"Tubes: {playerSystem.metalTubes}";
        wiringText.text = $"Wiring: {playerSystem.wiring}";
        circuitsText.text = $"Circuits: {playerSystem.circuits}";
    }
}

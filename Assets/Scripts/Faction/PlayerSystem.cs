using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerResourceSystem : MonoBehaviour
{
    [Header("Resources")]
    public float iron, energy, copper, silicon;
    public float ironRate = 10f, energyRate = 5f, copperRate = 3f, siliconRate = 2f;

    [Header("Components")]
    public int metalTubes, wiring, circuits;

    [Header("Ships")]
    public int lightShips, heavyShips, droneShips;
    public int TotalShips { get; private set; }

    public int lightRatio = 40, heavyRatio = 40, droneRatio = 20;
    public float spawnInterval = 1f;
    float spawnTimer;

    [Header("Spawn Settings")]
    public Transform planet;
    public GameObject lightShipPrefab, heavyShipPrefab, droneShipPrefab;
    public Transform enemyPlanet;

    [Header("UI")]
    public TextMeshProUGUI ironText, energyText, copperText, siliconText;
    public TextMeshProUGUI metalTubesText, wiringText, circuitsText;
    public TextMeshProUGUI lightShipsText, heavyShipsText, droneShipsText;
    public TextMeshProUGUI purchaseLogText;

    [Header("Sliders")]
    public Slider lightSlider, heavySlider, droneSlider;
    public TextMeshProUGUI lightRatioText, heavyRatioText, droneRatioText;
    private bool updatingSliders;

    void Start()
    {
        UpdateSliders();
        lightSlider.onValueChanged.AddListener(delegate { OnSliderChanged(); });
        heavySlider.onValueChanged.AddListener(delegate { OnSliderChanged(); });
        droneSlider.onValueChanged.AddListener(delegate { OnSliderChanged(); });
    }

    public void Tick()
    {
        GenerateResources();
        ManufactureComponents();
        HandleShipProduction();
        UpdateUI();
    }

    void GenerateResources()
    {
        iron += ironRate * Time.deltaTime;
        energy += energyRate * Time.deltaTime;
        copper += copperRate * Time.deltaTime;
        silicon += siliconRate * Time.deltaTime;
    }

    void ManufactureComponents()
    {
        if (iron >= 100 && energy >= 50) { iron -= 100; energy -= 50; metalTubes += 10; }
        if (copper >= 50 && silicon >= 20) { copper -= 50; silicon -= 20; wiring += 10; }
        if (iron >= 30 && copper >= 30 && silicon >= 40) { iron -= 30; copper -= 30; silicon -= 40; circuits += 5; }
    }

    void HandleShipProduction()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnShip();
        }
    }

    void SpawnShip()
    {
        int roll = Random.Range(1, 101);

        if (roll <= droneRatio && wiring >= 4 && circuits >= 4)
        {
            wiring -= 4; circuits -= 4;
            Spawn(droneShipPrefab, "Drone Ship");
        }
        else if (roll <= droneRatio + heavyRatio && metalTubes >= 10 && circuits >= 5)
        {
            metalTubes -= 10; circuits -= 5;
            Spawn(heavyShipPrefab, "Heavy Ship");
        }
        else if (metalTubes >= 5 && wiring >= 2)
        {
            metalTubes -= 5; wiring -= 2;
            Spawn(lightShipPrefab, "Light Ship");
        }
        else
        {
            purchaseLogText.text = "No Ship\nInsufficient components.";
        }
    }

    void Spawn(GameObject prefab, string name)
    {
        Instantiate(prefab, RandomOrbit(planet), Quaternion.identity).GetComponent<Ship>().SetupShip(enemyPlanet, true);
        purchaseLogText.text = $"Built: {name}";
        TotalShips++;
    }

    Vector3 RandomOrbit(Transform center, float radius = 2f)
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        return center.position + new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), 0);
    }

    void OnSliderChanged()
    {
        if (updatingSliders) return;
        float total = lightSlider.value + heavySlider.value + droneSlider.value;
        if (total == 0) total = 1;
        lightRatio = Mathf.RoundToInt((lightSlider.value / total) * 100);
        heavyRatio = Mathf.RoundToInt((heavySlider.value / total) * 100);
        droneRatio = 100 - (lightRatio + heavyRatio);
        UpdateSliders();
    }

    void UpdateSliders()
    {
        updatingSliders = true;
        lightSlider.value = lightRatio;
        heavySlider.value = heavyRatio;
        droneSlider.value = droneRatio;
        lightRatioText.text = $"{lightRatio}%";
        heavyRatioText.text = $"{heavyRatio}%";
        droneRatioText.text = $"{droneRatio}%";
        updatingSliders = false;
    }

    void UpdateUI()
    {
        ironText.text = $"Iron: {Mathf.FloorToInt(iron)}";
        energyText.text = $"Energy: {Mathf.FloorToInt(energy)}";
        copperText.text = $"Copper: {Mathf.FloorToInt(copper)}";
        siliconText.text = $"Silicon: {Mathf.FloorToInt(silicon)}";
        metalTubesText.text = $"Metal Tubes: {metalTubes}";
        wiringText.text = $"Wiring: {wiring}";
        circuitsText.text = $"Circuits: {circuits}";
        lightShipsText.text = $"Light: {lightShips}";
        heavyShipsText.text = $"Heavy: {heavyShips}";
        droneShipsText.text = $"Drone: {droneShips}";
    }

    public bool CanAffordShip(Shipyard.ShipyardType type)
    {
        switch (type)
        {
            case Shipyard.ShipyardType.Light:
                return metalTubes >= 5 && wiring >= 2;
            case Shipyard.ShipyardType.Heavy:
                return metalTubes >= 10 && circuits >= 5;
            case Shipyard.ShipyardType.Drone:
                return wiring >= 4 && circuits >= 4;
            default:
                return false;
        }
    }

    public void PayForShip(Shipyard.ShipyardType type)
    {
        switch (type)
        {
            case Shipyard.ShipyardType.Light:
                metalTubes -= 5;
                wiring -= 2;
                break;
            case Shipyard.ShipyardType.Heavy:
                metalTubes -= 10;
                circuits -= 5;
                break;
            case Shipyard.ShipyardType.Drone:
                wiring -= 4;
                circuits -= 4;
                break;
        }
    }

    public void IncrementShipCount()
    {
        TotalShips++;
    }

}

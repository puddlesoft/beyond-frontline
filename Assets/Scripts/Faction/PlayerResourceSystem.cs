using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerResourceSystem : MonoBehaviour
{
    [Header("Production Tracking")]

    private Queue<float> tubeHistory = new Queue<float>();
    private Queue<float> wiringHistory = new Queue<float>();
    private Queue<float> circuitHistory = new Queue<float>();

    private float tubeProducedThisSecond = 0;
    private float wiringProducedThisSecond = 0;
    private float circuitProducedThisSecond = 0;

    private float rollingUpdateTimer = 0f;
    private const int historySeconds = 5;


    // Shipyard tracking
    public int lightShipyards = 0;
    public int heavyShipyards = 0;
    public int droneShipyards = 0;

    [Header("Resources")]
    public float iron, energy, copper, silicon;
    public float ironRate = 10f, energyRate = 5f, copperRate = 3f, siliconRate = 2f;

    [Header("Components")]
    public int metalTubes, wiring, circuits;

    [Header("Ships")]
    public int TotalShips { get; private set; }


    [Header("Spawn Settings")]
    public Transform enemyPlanet;

    [Header("UI")]
    public TextMeshProUGUI ironText, energyText, copperText, siliconText;
    public TextMeshProUGUI metalTubesText, wiringText, circuitsText;

    void Start()
    {

    }

    public void Tick()
    {
        GenerateResources();
        ManufactureComponents();
        UpdateUI();
        UpdateProductionRates();

    }

    void GenerateResources()
    {
        iron += ironRate * Time.deltaTime;
        energy += energyRate * Time.deltaTime;
        copper += copperRate * Time.deltaTime;
        silicon += siliconRate * Time.deltaTime;
    }

    private enum ComponentType { Tubes, Wiring, Circuits }
    private ComponentType currentBuildTarget = ComponentType.Tubes;

    private float componentBuildTimer = 0f;
    private float componentCooldown = 1.5f; // Shared cooldown

    void ManufactureComponents()
    {
        componentBuildTimer += Time.deltaTime;
        if (componentBuildTimer < componentCooldown) return;

        switch (currentBuildTarget)
        {
            case ComponentType.Tubes:
                if (iron >= 100 && energy >= 50)
                {
                    iron -= 100;
                    energy -= 50;
                    metalTubes += 10;
                    tubeProducedThisSecond += 10;
                }
                break;

            case ComponentType.Wiring:
                if (copper >= 50 && silicon >= 20)
                {
                    copper -= 50;
                    silicon -= 20;
                    wiring += 10;
                    wiringProducedThisSecond += 10;
                }
                break;

            case ComponentType.Circuits:
                if (iron >= 30 && copper >= 30 && silicon >= 40)
                {
                    iron -= 30;
                    copper -= 30;
                    silicon -= 40;
                    circuits += 5;
                    circuitProducedThisSecond += 5;
                }
                break;
        }

        // Cycle to next component type
        currentBuildTarget = (ComponentType)(((int)currentBuildTarget + 1) % 3);
        componentBuildTimer = 0f;
    }

    void UpdateProductionRates()
    {
        rollingUpdateTimer += Time.deltaTime;
        if (rollingUpdateTimer >= 1f)
        {
            // Record new second
            tubeHistory.Enqueue(tubeProducedThisSecond);
            wiringHistory.Enqueue(wiringProducedThisSecond);
            circuitHistory.Enqueue(circuitProducedThisSecond);

            // Clamp to N seconds of history
            if (tubeHistory.Count > historySeconds) tubeHistory.Dequeue();
            if (wiringHistory.Count > historySeconds) wiringHistory.Dequeue();
            if (circuitHistory.Count > historySeconds) circuitHistory.Dequeue();

            // Reset this second's counters
            tubeProducedThisSecond = 0;
            wiringProducedThisSecond = 0;
            circuitProducedThisSecond = 0;
            rollingUpdateTimer = 0f;
        }
    }

    public float GetTubeRate() => AverageRate(tubeHistory);
    public float GetWiringRate() => AverageRate(wiringHistory);
    public float GetCircuitRate() => AverageRate(circuitHistory);

    float AverageRate(Queue<float> history)
    {
        if (history.Count == 0) return 0;
        float sum = 0;
        foreach (var val in history) sum += val;
        return sum / history.Count;
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

    public void GetRequiredRates(out float tubes, out float wiring, out float circuits)
    {
        float lightRate = 1f / 5f;   // Light builds every 5s
        float heavyRate = 1f / 6f;
        float droneRate = 1f / 7f;

        tubes = (lightShipyards * 5 * lightRate) + (heavyShipyards * 10 * heavyRate);
        wiring = (lightShipyards * 2 * lightRate) + (droneShipyards * 4 * droneRate);
        circuits = (heavyShipyards * 5 * heavyRate) + (droneShipyards * 4 * droneRate);
    }

    public void RegisterShipyard(Shipyard.ShipyardType type)
    {
        switch (type)
        {
            case Shipyard.ShipyardType.Light: lightShipyards++; break;
            case Shipyard.ShipyardType.Heavy: heavyShipyards++; break;
            case Shipyard.ShipyardType.Drone: droneShipyards++; break;
        }
    }
    public void DecrementShipCount()
    {
        TotalShips = Mathf.Max(0, TotalShips - 1);
    }

    private int nextTypeIndex = 0;
    private Shipyard.ShipyardType[] shipyardTypes = {
        Shipyard.ShipyardType.Light,
        Shipyard.ShipyardType.Heavy,
        Shipyard.ShipyardType.Drone
    };

    public bool TryGetNextBuildableType(out Shipyard.ShipyardType type)
    {
        for (int i = 0; i < shipyardTypes.Length; i++)
        {
            var current = shipyardTypes[nextTypeIndex];
            nextTypeIndex = (nextTypeIndex + 1) % shipyardTypes.Length;

            if (CanAffordShip(current))
            {
                type = current;
                return true;
            }
        }

        type = Shipyard.ShipyardType.Light; // default fallback
        return false;
    }




}

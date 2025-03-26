using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviour
{
    // Resources
    public float iron = 0;
    public float energy = 0;
    public float copper = 0;
    public float silicon = 0;

    // Components
    public int metalTubes = 0;
    public int wiring = 0;
    public int circuits = 0;

    // Ships
    public int lightShips = 0;
    public int heavyShips = 0;
    public int droneShips = 0;

    // Resource generation rates per second
    public float ironRate = 10f;
    public float energyRate = 5f;
    public float copperRate = 3f;
    public float siliconRate = 2f;

    // UI References
    public TextMeshProUGUI ironText, energyText, copperText, siliconText;
    public TextMeshProUGUI metalTubesText, wiringText, circuitsText;

    // Ship Counts
    public TextMeshProUGUI lightShipsText, heavyShipsText, droneShipsText;

    // Ship production ratios (%)
    public int lightShipRatio = 40;
    public int heavyShipRatio = 40;
    public int droneShipRatio = 20;

    // UI Slider References
    public Slider lightShipSlider;
    public Slider heavyShipSlider;
    public Slider droneShipSlider;

    // UI Slider Ratio Text References
    public TextMeshProUGUI lightShipRatioText;
    public TextMeshProUGUI heavyShipRatioText;
    public TextMeshProUGUI droneShipRatioText;

    // Purchase log reference
    public TextMeshProUGUI purchaseLogText;

    private bool slidersUpdating = false;

    public Transform planetPlayer, planetEnemy;
    public GameObject lightShipPrefab, heavyShipPrefab, droneShipPrefab;

    float shipBuildInterval = 1f;
    float shipBuildTimer = 0f;


    // Enemy Resources
    public float enemyIron = 0;
    public float enemyEnergy = 0;
    public float enemyCopper = 0;
    public float enemySilicon = 0;

    // Enemy Components
    public int enemyMetalTubes = 0;
    public int enemyWiring = 0;
    public int enemyCircuits = 0;

    // Enemy Ships
    public int enemyLightShips = 0;
    public int enemyHeavyShips = 0;
    public int enemyDroneShips = 0;

    // Enemy Resource generation rates per second
    public float enemyIronRate = 10f;
    public float enemyEnergyRate = 5f;
    public float enemyCopperRate = 3f;
    public float enemySiliconRate = 2f;

    // Enemy Ship production ratios (%)
    public int enemyLightShipRatio = 40;
    public int enemyHeavyShipRatio = 40;
    public int enemyDroneShipRatio = 20;

    float enemyShipBuildInterval = 1f;
    float enemyShipBuildTimer = 0f;

    // UI for total ship counts
    public TextMeshProUGUI playerShipCountText;
    public TextMeshProUGUI enemyShipCountText;

    // Internal tracking for ship counts
    public int playerTotalShips = 0;
    public int enemyTotalShips = 0;



    void Start()
    {
        UpdateSliderUI();
        lightShipSlider.onValueChanged.AddListener(delegate { OnSliderChanged(); });
        heavyShipSlider.onValueChanged.AddListener(delegate { OnSliderChanged(); });
        droneShipSlider.onValueChanged.AddListener(delegate { OnSliderChanged(); });
    }

    void UpdateSliderUI()
    {
        slidersUpdating = true;

        lightShipSlider.value = lightShipRatio;
        heavyShipSlider.value = heavyShipRatio;
        droneShipSlider.value = droneShipRatio;

        UpdateRatioTexts();

        slidersUpdating = false;
    }

    public void OnSliderChanged()
    {
        if (slidersUpdating) return;

        float total = lightShipSlider.value + heavyShipSlider.value + droneShipSlider.value;

        if (total == 0) total = 1;

        lightShipRatio = Mathf.RoundToInt((lightShipSlider.value / total) * 100);
        heavyShipRatio = Mathf.RoundToInt((heavyShipSlider.value / total) * 100);
        droneShipRatio = 100 - (lightShipRatio + heavyShipRatio);

        UpdateSliderUI();
    }

    void UpdateRatioTexts()
    {
        lightShipRatioText.text = $"{lightShipRatio}%";
        heavyShipRatioText.text = $"{heavyShipRatio}%";
        droneShipRatioText.text = $"{droneShipRatio}%";
    }

    void Update()
    {
        GenerateResources();
        ManufactureComponents();
        HandleShipProduction();

        GenerateEnemyResources();
        ManufactureEnemyComponents();
        HandleEnemyShipProduction();

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
        if (iron >= 100f && energy >= 50f)
        {
            iron -= 100f;
            energy -= 50f;
            metalTubes += 10;
        }

        if (copper >= 50f && silicon >= 20f)
        {
            copper -= 50f;
            silicon -= 20f;
            wiring += 10;
        }

        if (iron >= 30f && copper >= 30f && silicon >= 40f)
        {
            iron -= 30f;
            copper -= 30f;
            silicon -= 40f;
            circuits += 5;
        }
    }

    void HandleShipProduction()
    {
        shipBuildTimer += Time.deltaTime;

        if (shipBuildTimer >= shipBuildInterval)
        {
            shipBuildTimer = 0f;
            AttemptShipProduction();
        }
    }

    void AttemptShipProduction()
    {
        int randomPick = Random.Range(1, 101);

        if (randomPick <= droneShipRatio)
        {
            if (CanBuildDrone())
                BuildDroneShip();
            else
                LogPurchase("No Ship", "Insufficient components for Drone Ship");
        }
        else if (randomPick <= droneShipRatio + heavyShipRatio)
        {
            if (CanBuildHeavy())
                BuildHeavyShip();
            else
                LogPurchase("No Ship", "Insufficient components for Heavy Ship");
        }
        else
        {
            if (CanBuildLight())
                BuildLightShip();
            else
                LogPurchase("No Ship", "Insufficient components for Light Ship");
        }
    }

    // Helper methods to check and build ships clearly:

    bool CanBuildLight() => metalTubes >= 5 && wiring >= 2;
    bool CanBuildHeavy() => metalTubes >= 10 && circuits >= 5;
    bool CanBuildDrone() => wiring >= 4 && circuits >= 4;

    void BuildLightShip()
    {
        metalTubes -= 5;
        wiring -= 2;
        lightShips++;
        playerTotalShips++;
        LogPurchase("Light Ship", "5 Metal Tubes, 2 Wiring");
        GameObject newShip = Instantiate(
            lightShipPrefab,
            GetRandomOrbitPosition(planetPlayer),
            Quaternion.identity
        );
        Ship shipScript = newShip.GetComponent<Ship>();
        shipScript.targetPlanet = planetEnemy;
        shipScript.isPlayerShip = true;
    }

    void BuildHeavyShip()
    {
        metalTubes -= 10;
        circuits -= 5;
        heavyShips++;
        playerTotalShips++;
        LogPurchase("Heavy Ship", "10 Metal Tubes, 5 Circuits");
        GameObject newShip = Instantiate(
            heavyShipPrefab,
            GetRandomOrbitPosition(planetPlayer),
            Quaternion.identity
        );
        Ship shipScript = newShip.GetComponent<Ship>();
        shipScript.targetPlanet = planetEnemy;
        shipScript.isPlayerShip = true;
    }

    void BuildDroneShip()
    {
        wiring -= 4;
        circuits -= 4;
        droneShips++;
        playerTotalShips++;
        LogPurchase("Drone Ship", "4 Wiring, 4 Circuits");
        GameObject newShip = Instantiate(
            droneShipPrefab,
            GetRandomOrbitPosition(planetPlayer),
            Quaternion.identity
        );
        Ship shipScript = newShip.GetComponent<Ship>();
        shipScript.targetPlanet = planetEnemy;
        shipScript.isPlayerShip = true;
    }

    void LogPurchase(string shipType, string componentsUsed)
    {
        purchaseLogText.text = $"Built: {shipType}\nUsed: {componentsUsed}";
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

        // Update total ship count UI clearly
        playerShipCountText.text = $"Player Ships: {playerTotalShips}";
        enemyShipCountText.text = $"Enemy Ships: {enemyTotalShips}";
    }

    Vector3 GetRandomOrbitPosition(Transform planet, float radius = 2f)
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        float x = planet.position.x + radius * Mathf.Cos(angle);
        float y = planet.position.y + radius * Mathf.Sin(angle);
        return new Vector3(x, y, 0);
    }

    void GenerateEnemyResources()
    {
        enemyIron += enemyIronRate * Time.deltaTime;
        enemyEnergy += enemyEnergyRate * Time.deltaTime;
        enemyCopper += enemyCopperRate * Time.deltaTime;
        enemySilicon += enemySiliconRate * Time.deltaTime;
    }

    void ManufactureEnemyComponents()
    {
        if (enemyIron >= 100f && enemyEnergy >= 50f)
        {
            enemyIron -= 100f;
            enemyEnergy -= 50f;
            enemyMetalTubes += 10;
        }

        if (enemyCopper >= 50f && enemySilicon >= 20f)
        {
            enemyCopper -= 50f;
            enemySilicon -= 20f;
            enemyWiring += 10;
        }

        if (enemyIron >= 30f && enemyCopper >= 30f && enemySilicon >= 40f)
        {
            enemyIron -= 30f;
            enemyCopper -= 30f;
            enemySilicon -= 40f;
            enemyCircuits += 5;
        }
    }

    void HandleEnemyShipProduction()
    {
        enemyShipBuildTimer += Time.deltaTime;

        if (enemyShipBuildTimer >= enemyShipBuildInterval)
        {
            enemyShipBuildTimer = 0f;
            AttemptEnemyShipProduction();
        }
    }

    void AttemptEnemyShipProduction()
    {
        int randomPick = Random.Range(1, 101);

        if (randomPick <= enemyDroneShipRatio)
        {
            if (CanBuildEnemyDrone())
                BuildEnemyDroneShip();
        }
        else if (randomPick <= enemyDroneShipRatio + enemyHeavyShipRatio)
        {
            if (CanBuildEnemyHeavy())
                BuildEnemyHeavyShip();
        }
        else
        {
            if (CanBuildEnemyLight())
                BuildEnemyLightShip();
        }
    }

    bool CanBuildEnemyLight() => enemyMetalTubes >= 5 && enemyWiring >= 2;
    bool CanBuildEnemyHeavy() => enemyMetalTubes >= 10 && enemyCircuits >= 5;
    bool CanBuildEnemyDrone() => enemyWiring >= 4 && enemyCircuits >= 4;

    void BuildEnemyLightShip()
    {
        enemyMetalTubes -= 5;
        enemyWiring -= 2;
        enemyLightShips++;
        enemyTotalShips++;

        GameObject newShip = Instantiate(lightShipPrefab, GetRandomOrbitPosition(planetEnemy), Quaternion.identity);
        Ship shipScript = newShip.GetComponent<Ship>();
        shipScript.targetPlanet = planetPlayer;
        shipScript.isPlayerShip = false;
    }

    void BuildEnemyHeavyShip()
    {
        enemyMetalTubes -= 10;
        enemyCircuits -= 5;
        enemyHeavyShips++;
        enemyTotalShips++;

        GameObject newShip = Instantiate(heavyShipPrefab, GetRandomOrbitPosition(planetEnemy), Quaternion.identity);
        Ship shipScript = newShip.GetComponent<Ship>();
        shipScript.targetPlanet = planetPlayer;
        shipScript.isPlayerShip = false;
    }

    void BuildEnemyDroneShip()
    {
        enemyWiring -= 4;
        enemyCircuits -= 4;
        enemyDroneShips++;
        enemyTotalShips++;

        GameObject newShip = Instantiate(heavyShipPrefab, GetRandomOrbitPosition(planetEnemy), Quaternion.identity);
        Ship shipScript = newShip.GetComponent<Ship>();
        shipScript.targetPlanet = planetPlayer;
        shipScript.isPlayerShip = false;
    }

}

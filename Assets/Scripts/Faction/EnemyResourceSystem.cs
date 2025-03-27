using UnityEngine;

public class EnemyResourceSystem : MonoBehaviour
{
    public float iron, energy, copper, silicon;
    public float ironRate = 10f, energyRate = 5f, copperRate = 3f, siliconRate = 2f;

    public int metalTubes, wiring, circuits;
    public int TotalShips { get; private set; }

    public int lightRatio = 40, heavyRatio = 40, droneRatio = 20;
    public float spawnInterval = 1f;
    private float spawnTimer;

    public Transform planet;
    public Transform targetPlanet;
    public GameObject lightShipPrefab, heavyShipPrefab, droneShipPrefab;

    public void Tick()
    {
        GenerateResources();
        ManufactureComponents();
        HandleShipProduction();
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
        if (spawnTimer < spawnInterval) return;
        spawnTimer = 0f;

        int roll = Random.Range(1, 101);
        if (roll <= droneRatio && wiring >= 4 && circuits >= 4)
            Spawn(droneShipPrefab, 0, 4, 4);
        else if (roll <= droneRatio + heavyRatio && metalTubes >= 10 && circuits >= 5)
            Spawn(heavyShipPrefab, 10, 0, 5);
        else if (metalTubes >= 5 && wiring >= 2)
            Spawn(lightShipPrefab, 5, 2, 0);
    }

    void Spawn(GameObject prefab, int tubes, int wires, int circs)
    {
        metalTubes -= tubes;
        wiring -= wires;
        circuits -= circs;

        GameObject shipObj = Instantiate(prefab, RandomOrbit(planet), Quaternion.identity);
        Ship ship = shipObj.GetComponent<Ship>();
        ship.SetupShip(targetPlanet, false, null, this);         // Mark as enemy
        ship.enemySystem = this;                     // Hook for count decrementing

        TotalShips++;
    }

    Vector3 RandomOrbit(Transform center, float radius = 2f)
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        return center.position + new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle), 0);
    }

    public void DecrementShipCount()
    {
        TotalShips = Mathf.Max(0, TotalShips - 1);
    }
}

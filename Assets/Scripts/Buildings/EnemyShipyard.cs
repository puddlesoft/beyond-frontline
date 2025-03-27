using UnityEngine;

public class EnemyShipyard : MonoBehaviour
{
    public enum ShipyardType { Light, Heavy, Drone }
    public ShipyardType shipyardType;

    public float buildInterval = 5f;
    private float buildTimer;

    public GameObject shipPrefab;
    public Transform targetPlanet;

    private EnemyResourceSystem enemySystem;

    public void Initialize(EnemyResourceSystem system, Transform enemyTarget)
    {
        enemySystem = system;
        targetPlanet = enemyTarget;
        buildTimer = buildInterval;
    }

    void Update()
    {
        if (enemySystem == null) return;

        buildTimer -= Time.deltaTime;
        if (buildTimer > 0f) return;

        if (CanAfford())
        {
            PayCost();
            SpawnShip();
            buildTimer = buildInterval;
        }
    }

    void SpawnShip()
    {
        GameObject ship = Instantiate(shipPrefab, transform.position, Quaternion.identity);
        Ship shipScript = ship.GetComponent<Ship>();
        shipScript.SetupShip(targetPlanet, false, null, enemySystem);
        shipScript.SetEnemySystem(enemySystem);

        enemySystem.IncrementShipCount();
    }

    bool CanAfford()
    {
        switch (shipyardType)
        {
            case ShipyardType.Light:
                return enemySystem.metalTubes >= 5 && enemySystem.wiring >= 2;
            case ShipyardType.Heavy:
                return enemySystem.metalTubes >= 10 && enemySystem.circuits >= 5;
            case ShipyardType.Drone:
                return enemySystem.wiring >= 4 && enemySystem.circuits >= 4;
        }
        return false;
    }

    void PayCost()
    {
        switch (shipyardType)
        {
            case ShipyardType.Light:
                enemySystem.metalTubes -= 5;
                enemySystem.wiring -= 2;
                break;
            case ShipyardType.Heavy:
                enemySystem.metalTubes -= 10;
                enemySystem.circuits -= 5;
                break;
            case ShipyardType.Drone:
                enemySystem.wiring -= 4;
                enemySystem.circuits -= 4;
                break;
        }
    }
}

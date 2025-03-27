using UnityEngine;

public class Shipyard : MonoBehaviour
{
    public enum ShipyardType { Light, Heavy, Drone }
    public ShipyardType shipyardType;

    public float buildInterval = 5f;
    private float buildTimer;

    [Header("Spawn Settings")]
    public GameObject shipPrefab;
    public Transform targetPlanet;

    private PlayerResourceSystem playerSystem;
    private EnemyResourceSystem enemySystem;
    private bool isPlayer = true;

    public void Initialize(PlayerResourceSystem system, Transform enemyPlanet, bool isPlayerShip = true, EnemyResourceSystem enemy = null)
    {
        targetPlanet = enemyPlanet;
        playerSystem = system;
        enemySystem = enemy;
        isPlayer = isPlayerShip;
        buildTimer = buildInterval;

        if (isPlayer && playerSystem != null)
        {
            playerSystem.RegisterShipyard(shipyardType);
        }
    }

    void Update()
    {
        buildTimer -= Time.deltaTime;
        if (buildTimer > 0f) return;

        if (isPlayer)
        {
            if (playerSystem != null && playerSystem.CanAffordShip(shipyardType))
            {
                playerSystem.PayForShip(shipyardType);
                SpawnShip(true);
                buildTimer = buildInterval;
            }
        }
        else
        {
            if (enemySystem != null && CanEnemyAfford())
            {
                PayEnemyCost();
                SpawnShip(false);
                buildTimer = buildInterval;
            }
        }
    }

    void SpawnShip(bool isPlayerShip)
    {
        GameObject ship = Instantiate(shipPrefab, transform.position, Quaternion.identity);
        ship.GetComponent<Ship>().SetupShip(targetPlanet, isPlayerShip, playerSystem, enemySystem);

        if (isPlayerShip && playerSystem != null)
        {
            playerSystem.IncrementShipCount();
        }
        else if (!isPlayerShip && enemySystem != null)
        {
            enemySystem.IncrementShipCount();
        }
    }

    bool CanEnemyAfford()
    {
        switch (shipyardType)
        {
            case ShipyardType.Light:
                return enemySystem.metalTubes >= 5 && enemySystem.wiring >= 2;
            case ShipyardType.Heavy:
                return enemySystem.metalTubes >= 10 && enemySystem.circuits >= 5;
            case ShipyardType.Drone:
                return enemySystem.wiring >= 4 && enemySystem.circuits >= 4;
            default:
                return false;
        }
    }

    void PayEnemyCost()
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

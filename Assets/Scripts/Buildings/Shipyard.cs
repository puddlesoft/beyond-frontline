using UnityEngine;

public class Shipyard : MonoBehaviour
{
    public enum ShipyardType { Light, Heavy, Drone }
    public ShipyardType shipyardType;

    public GameObject shipPrefab;
    public Transform targetPlanet;

    private PlayerResourceSystem playerSystem;
    private EnemyResourceSystem enemySystem;
    private bool isPlayer = true;

    private float buildCooldown = 5f;
    private float buildTimer = 0f;

    void Update()
    {
        buildTimer += Time.deltaTime;

        if (buildTimer >= buildCooldown)
        {
            buildTimer = 0f;

            if (isPlayer)
            {
                playerSystem?.EnqueueShipBuild(this);
            }
            else
            {
                enemySystem?.EnqueueShipBuild(this);
            }
        }
    }

    public ShipyardType GetShipyardType()
    {
        return shipyardType;
    }
    public void Initialize(PlayerResourceSystem player, Transform enemyPlanet, bool isPlayerShip = true, EnemyResourceSystem enemy = null)
    {
        targetPlanet = enemyPlanet;
        playerSystem = player;
        enemySystem = enemy;
        isPlayer = isPlayerShip;

        Debug.Log($"[Shipyard] Initializing {(isPlayer ? "Player" : "Enemy")} yard. TargetPlanet: {enemyPlanet?.name}");

        if (isPlayer && playerSystem != null)
        {
            playerSystem.productionManager.RegisterShipyard(shipyardType, this);
        }
        else if (!isPlayer && enemySystem != null)
        {
            enemySystem.productionManager.RegisterShipyard(shipyardType, this);
        }
        else
        {
            Debug.LogWarning($"[Shipyard] Missing system during init. Player: {playerSystem != null}, Enemy: {enemySystem != null}");
        }
    }
    public bool CanPlayerAfford(PlayerResourceSystem system)
    {
        if (system == null) return false;

        switch (shipyardType)
        {
            case ShipyardType.Light:
                return system.metalTubes >= 5 && system.wiring >= 2;
            case ShipyardType.Heavy:
                return system.metalTubes >= 10 && system.circuits >= 5;
            case ShipyardType.Drone:
                return system.wiring >= 4 && system.circuits >= 4;
        }

        return false;
    }

    public bool CanEnemyAfford(EnemyResourceSystem system)
    {
        if (system == null) return false;

        switch (shipyardType)
        {
            case ShipyardType.Light:
                return system.metalTubes >= 5 && system.wiring >= 2;
            case ShipyardType.Heavy:
                return system.metalTubes >= 10 && system.circuits >= 5;
            case ShipyardType.Drone:
                return system.wiring >= 4 && system.circuits >= 4;
        }

        return false;
    }

    public void SpawnShip(bool isPlayer)
    {
        GameObject ship = Instantiate(shipPrefab, transform.position, Quaternion.identity);
        Ship shipComponent = ship.GetComponent<Ship>();

        if (shipComponent == null)
        {
            Debug.LogError($"[Shipyard] Spawned object is missing Ship component! ({shipPrefab.name})");
            return;
        }

        if (targetPlanet == null)
        {
            Debug.LogError($"[Shipyard] targetPlanet is NULL for {(isPlayer ? "player" : "enemy")} shipyard!");
        }

        Debug.Log($"[Shipyard] Spawning ship from {(isPlayer ? "player" : "enemy")} shipyard at {transform.position}");

        if (isPlayer && playerSystem != null)
        {
            playerSystem.PayForShip(shipyardType);
            shipComponent.SetupShip(targetPlanet, true, playerSystem, null);
            playerSystem.IncrementShipCount();
        }
        else if (!isPlayer && enemySystem != null)
        {
            enemySystem.PayForShip(shipyardType);
            shipComponent.SetupShip(targetPlanet, false, null, enemySystem);
            enemySystem.IncrementShipCount();
        }
    }





}

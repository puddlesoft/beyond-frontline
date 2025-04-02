using UnityEngine;

public class Shipyard : MonoBehaviour
{
    public enum ShipyardType { Light, Heavy, Drone }
    public ShipyardType shipyardType;

    public GameObject shipPrefab;
    public Transform targetPlanet;

    private PlayerResourceSystem playerSystem;
    private EnemyResourceSystem enemySystem;
    public bool isPlayer { get; private set; } = true;

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

        // If we're an enemy shipyard and don't have a target, find the nearest player planet
        if (!isPlayer && targetPlanet == null)
        {
            targetPlanet = FindNearestPlayerPlanet();
            if (targetPlanet == null)
            {
                Debug.LogError($"[Shipyard] Could not find a player planet for enemy shipyard to target!");
                return;
            }
        }

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
            Debug.LogError($"[Shipyard] Missing system during init. Player: {playerSystem != null}, Enemy: {enemySystem != null}");
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

        Transform target = targetPlanet;
        if (!isPlayer && target == null)
        {
            target = FindNearestPlayerPlanet();
            if (target == null)
            {
                Debug.LogError($"[Shipyard] Could not find a player planet for enemy ship to target!");
                return;
            }
        }

        if (isPlayer && playerSystem != null)
        {
            playerSystem.PayForShip(shipyardType);
            shipComponent.SetupShip(target, true, playerSystem, null);
            playerSystem.IncrementShipCount();
        }
        else if (!isPlayer && enemySystem != null)
        {
            enemySystem.PayForShip(shipyardType);
            shipComponent.SetupShip(target, false, null, enemySystem);
            enemySystem.IncrementShipCount();
        }
        else
        {
            Debug.LogError($"[Shipyard] Missing system during ship spawn. Player: {playerSystem != null}, Enemy: {enemySystem != null}");
            return;
        }

        // Update debug state for the new ship
        var gameManager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            var shipCombat = ship.GetComponent<ShipCombat>();
            if (shipCombat != null)
            {
                shipCombat.showDebug = gameManager.showShipDebug;
                Debug.Log($"[Shipyard] Updated debug state for {ship.name}: {gameManager.showShipDebug}");
            }
        }
    }

    private Transform FindNearestPlayerPlanet()
    {
        Transform nearest = null;
        float nearestDist = Mathf.Infinity;

        // Find all planets in the scene
        var planets = GameObject.FindGameObjectsWithTag("Planet");
        foreach (var planet in planets)
        {
            // Check if it's a player planet
            var planetComponent = planet.GetComponent<Planet>();
            if (planetComponent != null && planetComponent.IsPlayerPlanet())
            {
                float dist = Vector3.Distance(transform.position, planet.transform.position);
                if (dist < nearestDist)
                {
                    nearest = planet.transform;
                    nearestDist = dist;
                }
            }
        }

        if (nearest == null)
        {
            Debug.LogError($"[Shipyard] No player planets found for enemy ship to target!");
        }

        return nearest;
    }
}

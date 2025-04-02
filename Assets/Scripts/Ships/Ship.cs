using UnityEngine;

[RequireComponent(typeof(ShipMovement), typeof(ShipCombat))]
public class Ship : MonoBehaviour
{
    public enum ShipType { Light, Heavy, Drone }
    public ShipType shipType;

    public Transform targetPlanet;
    public GameObject projectilePrefab;

    public float currentHP;
    public float maxHP;
    public float fireCooldown;
    public bool isPlayerShip;

    private PlayerResourceSystem playerSystem;
    private EnemyResourceSystem enemySystem;

    private ShipMovement movement;
    private ShipCombat combat;
    private ShipStateMachine stateMachine;
    private bool isInitialized = false;

    void Awake()
    {
        // Add required components if they don't exist
        if (movement == null)
        {
            movement = gameObject.AddComponent<ShipMovement>();
        }

        if (combat == null)
        {
            combat = gameObject.AddComponent<ShipCombat>();
        }

        if (stateMachine == null)
        {
            stateMachine = gameObject.AddComponent<ShipStateMachine>();
        }
    }

    void Start()
    {
        // Verify components exist
        if (movement == null || combat == null || stateMachine == null)
        {
            Debug.LogError($"[Ship] {name} missing required components in Start");
        }
    }

    void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        if (movement == null || combat == null)
        {
            Debug.LogWarning($"[Ship] {name} missing components at runtime");
            return;
        }

        // Update combat first to calculate new positions
        combat.Tick();
        // Then update movement to apply those positions
        movement.Tick();
    }

    public void SetupShip(Transform target, bool isPlayer, PlayerResourceSystem player = null, EnemyResourceSystem enemy = null)
    {
        if (isInitialized)
        {
            Debug.LogWarning($"[Ship] {name} already initialized, skipping");
            return;
        }

        targetPlanet = target;
        isPlayerShip = isPlayer;
        playerSystem = player;
        enemySystem = enemy;

        // For enemy ships, find the nearest player planet if no target is specified
        if (!isPlayer && targetPlanet == null)
        {
            targetPlanet = FindNearestPlayerPlanet();
            if (targetPlanet == null)
            {
                Debug.LogError($"[Ship] Could not find target planet for enemy ship {name}!");
                return;
            }
        }

        // Initialize components in correct order
        SetShipStats();
        
        // Initialize state machine first since other components might need it
        stateMachine.Initialize();
        
        // Initialize combat since it needs the ship reference
        combat.Initialize(this, projectilePrefab);
        
        // Then initialize movement with the target planet
        movement.Initialize(targetPlanet);
        
        // Finally set the initial state
        stateMachine.SetState(ShipState.Moving);
        
        isInitialized = true;
        Debug.Log($"[Ship] {name} initialized as {(isPlayer ? "Player" : "Enemy")} ship, targeting {targetPlanet.name}");
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
            Debug.LogError($"[Ship] No player planets found for enemy ship {name} to target!");
        }

        return nearest;
    }

    void SetShipStats()
    {
        switch (shipType)
        {
            case ShipType.Light: maxHP = currentHP = 50; fireCooldown = 1f; break;
            case ShipType.Heavy: maxHP = currentHP = 150; fireCooldown = 1.5f; break;
            case ShipType.Drone: maxHP = currentHP = 100; fireCooldown = 2f; break;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            if (isPlayerShip) playerSystem?.DecrementShipCount();
            else enemySystem?.DecrementShipCount();

            if (stateMachine != null)
            {
                stateMachine.SetState(ShipState.Dead);
            }
            Destroy(gameObject, 0.1f); // Small delay to allow state change to process
        }
    }

    public ShipType GetShipType() => shipType;
    public bool IsPlayer() => isPlayerShip;
}

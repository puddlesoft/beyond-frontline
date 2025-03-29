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

    public void SetEnemySystem(EnemyResourceSystem system)
    {
        enemySystem = system;
    }

    void Start()
    {
        movement = GetComponent<ShipMovement>();
        combat = GetComponent<ShipCombat>();
    }

    void Update()
    {
        if (!isInitialized) return;

        if (movement == null || combat == null)
        {
            Debug.LogWarning($"[Ship] {name} missing components at runtime. Movement: {(movement != null)}, Combat: {(combat != null)}");
            return;
        }

        movement.Tick();
        combat.Tick();
    }

    public void SetupShip(Transform target, bool isPlayer, PlayerResourceSystem player = null, EnemyResourceSystem enemy = null)
    {
        targetPlanet = target;
        isPlayerShip = isPlayer;
        playerSystem = player;
        enemySystem = enemy;

        Debug.Log($"[Ship] SetupShip called for {(isPlayer ? "Player" : "Enemy")}. Target: {targetPlanet?.name}");

        movement = GetComponent<ShipMovement>();
        combat = GetComponent<ShipCombat>();

        stateMachine = gameObject.AddComponent<ShipStateMachine>();

        if (movement == null)
        {
            Debug.LogError($"[Ship] MISSING ShipMovement on {name}! Did you forget to add it to the prefab?");
        }
        else
        {
            movement.Initialize(targetPlanet);
            Debug.Log($"[Ship] Movement initialized for {name}, target = {targetPlanet?.name}");
        }

        if (combat == null)
        {
            Debug.LogError($"[Ship] MISSING ShipCombat on {name}! Did you forget to add it to the prefab?");
        }
        else
        {
            if (shipType != ShipType.Light && projectilePrefab == null)
            {
                Debug.LogError($"[Ship] MISSING projectilePrefab for {shipType} on {name}!");
            }

            combat.Initialize(this, projectilePrefab);
            Debug.Log($"[Ship] Combat initialized for {name}. Projectile: {(projectilePrefab != null ? projectilePrefab.name : "None")}");
        }

        SetShipStats();

        if (movement != null && movement.GetCurrentTarget() == null)
        {
            Debug.LogWarning($"[Ship] moveTarget was still null after init on {name}. Re-assigning to targetPlanet: {targetPlanet?.name}");
            movement.SetMoveTarget(targetPlanet);
        }

        if (stateMachine != null)
        {
            stateMachine.SetState(ShipState.Moving);
        }

        isInitialized = true;
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

            Destroy(gameObject);
        }
    }

    public ShipType GetShipType() => shipType;
    public bool IsPlayer() => isPlayerShip;
}

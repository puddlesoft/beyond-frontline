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

    public void SetEnemySystem(EnemyResourceSystem system)
    {
        enemySystem = system;
    }

    public void SetupShip(Transform target, bool isPlayer, PlayerResourceSystem player = null, EnemyResourceSystem enemy = null)
    {
        targetPlanet = target;
        isPlayerShip = isPlayer;
        playerSystem = player;
        enemySystem = enemy;

        movement = GetComponent<ShipMovement>();
        combat = GetComponent<ShipCombat>();

        SetShipStats();
        movement.Initialize(targetPlanet);
        combat.Initialize(this, projectilePrefab);
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

    void Update()
    {
        movement.Tick();
        combat.Tick();
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

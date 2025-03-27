using UnityEngine;
using System.Collections;

public class Ship : MonoBehaviour
{
    public enum ShipType { Light, Heavy, Drone }
    public ShipType shipType;
    public Transform targetPlanet;
    private PlayerResourceSystem playerSystem;
    public EnemyResourceSystem enemySystem;

    public float moveSpeed = 2f;

    public float detectRange = 4f;
    public float attackRange = 2f;

    public float fireCooldown;
    public float currentHP;
    public float maxHP;
    public bool isPlayerShip;
    public GameObject projectilePrefab;

    private Transform currentTarget;
    private bool isFiring;

    void Start()
    {
        SetShipStats();
    }

    public void SetupShip(Transform target, bool isPlayer, PlayerResourceSystem player = null, EnemyResourceSystem enemy = null)
    {
        targetPlanet = target;
        isPlayerShip = isPlayer;
        playerSystem = player;
        enemySystem = enemy;
    }


    void Update()
    {
        if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.position) > detectRange)
        {
            FindTarget();
        }

        if (currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

            if (distanceToTarget <= attackRange)
            {
                if (!isFiring)
                {
                    StartCoroutine(FireAtTarget());
                }
            }
            else
            {
                MoveTowards(currentTarget.position); // <-- Important line added
            }
        }
        else if (targetPlanet != null)
        {
            MoveTowards(targetPlanet.position);
        }
    }


    void SetShipStats()
    {
        switch (shipType)
        {
            case ShipType.Light:
                maxHP = currentHP = 50;
                fireCooldown = 1f;
                break;
            case ShipType.Heavy:
                maxHP = currentHP = 150;
                fireCooldown = 1.5f;
                break;
            case ShipType.Drone:
                maxHP = currentHP = 100;
                fireCooldown = 2f;
                break;
        }
    }

    void MoveTowards(Vector3 targetPos)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }

    void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRange);
        Transform closestShip = null;
        Transform closestTurret = null;
        float closestShipDist = Mathf.Infinity;
        float closestTurretDist = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Ship enemyShip = hit.GetComponent<Ship>();
            if (enemyShip != null && enemyShip.targetPlanet != targetPlanet)
            {
                float dist = Vector3.Distance(transform.position, enemyShip.transform.position);
                if (dist < closestShipDist)
                {
                    closestShip = enemyShip.transform;
                    closestShipDist = dist;
                }
                continue;
            }

            DefenseTurret turret = hit.GetComponent<DefenseTurret>();
            if (turret != null && turret.gameObject.activeInHierarchy)
            {
                if (turret.isPlayerTurret != isPlayerShip)
                {
                    float dist = Vector3.Distance(transform.position, turret.transform.position);
                    if (dist < closestTurretDist)
                    {
                        closestTurret = turret.transform;
                        closestTurretDist = dist;
                    }
                }
            }

        }

        if (closestShip != null)
        {
            currentTarget = closestShip;
        }
        else if (closestTurret != null)
        {
            currentTarget = closestTurret;
        }
        else
        {
            currentTarget = null;
        }
    }

    IEnumerator FireAtTarget()
    {
        isFiring = true;

        while (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
        {
            ShootProjectile(currentTarget);
            yield return new WaitForSeconds(fireCooldown);
        }

        isFiring = false;
    }

    void ShootProjectile(Transform target)
    {
        if (shipType == ShipType.Light)
        {
            // Hitscan laser logic
            if (target.TryGetComponent<Ship>(out Ship enemyShip))
            {
                enemyShip.TakeDamage(GetDamageAgainst(shipType, enemyShip.shipType));
                DrawLaserBeam(target.position);
            }
            else if (target.TryGetComponent<DefenseTurret>(out DefenseTurret turret))
            {
                turret.TakeDamage(GetDamageAgainst(shipType, ShipType.Heavy)); // treat as heavy target
                DrawLaserBeam(target.position);
            }
        }
    }

    float GetDamageAgainst(ShipType shooter, ShipType targetType)
    {
        if (shooter == ShipType.Light)
            return targetType == ShipType.Heavy ? 25 : 10;
        if (shooter == ShipType.Heavy)
            return targetType == ShipType.Drone ? 30 : 15;
        if (shooter == ShipType.Drone)
            return targetType == ShipType.Light ? 35 : 15;

        return 10;
    }

    void DrawLaserBeam(Vector3 targetPosition)
    {
        GameObject laser = new GameObject("LaserBeam");
        LineRenderer lineRenderer = laser.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, targetPosition);

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.cyan;
        lineRenderer.endColor = Color.blue;

        Destroy(laser, 0.1f);
    }

    public void TakeDamage(float amount)
{
    currentHP -= amount;
    if (currentHP <= 0)
    {
        if (isPlayerShip && playerSystem != null)
        {
            playerSystem.DecrementShipCount();
        }
        else if (!isPlayerShip && enemySystem != null)
        {
            enemySystem.DecrementShipCount();
        }

        Destroy(gameObject);
    }
}
}

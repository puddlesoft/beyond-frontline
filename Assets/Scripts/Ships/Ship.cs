using UnityEngine;
using System.Collections;

public class Ship : MonoBehaviour
{
    public enum ShipType { Light, Heavy, Drone }
    public ShipType shipType;

    public Transform targetPlanet;
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

    public void SetupShip(Transform target, bool isPlayer)
    {
        targetPlanet = target;
        isPlayerShip = isPlayer;
    }

    void Update()
    {
        if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.position) > detectRange)
            FindTarget();

        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
        {
            if (!isFiring)
                StartCoroutine(FireAtTarget());
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
        Transform preferredTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Ship enemyShip = hit.GetComponent<Ship>();
            if (enemyShip != null && enemyShip.targetPlanet != targetPlanet)
            {
                float distance = Vector3.Distance(transform.position, enemyShip.transform.position);
                if (preferredTarget == null || IsPreferredTarget(enemyShip))
                {
                    if (distance < closestDistance)
                    {
                        preferredTarget = enemyShip.transform;
                        closestDistance = distance;
                    }
                }
            }
        }
        currentTarget = preferredTarget;
    }

    bool IsPreferredTarget(Ship enemyShip)
    {
        return (shipType == ShipType.Light && enemyShip.shipType == ShipType.Heavy) ||
               (shipType == ShipType.Heavy && enemyShip.shipType == ShipType.Drone) ||
               (shipType == ShipType.Drone && enemyShip.shipType == ShipType.Light);
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
            Ship enemy = target.GetComponent<Ship>();
            if (enemy != null)
            {
                enemy.TakeDamage(GetDamageAgainst(shipType, enemy.shipType));
                DrawLaserBeam(target.position);
            }
        }
        else
        {
            // Projectile logic for Heavy and Drone
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Projectile projectile = proj.GetComponent<Projectile>();
            projectile.SetTarget(target, shipType);
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
            Destroy(gameObject);
        }
    }
}

using UnityEngine;
using System.Collections;

public class ShipCombat : MonoBehaviour
{
    private Ship ship;
    private ShipMovement movement;
    private GameObject projectilePrefab;

    public float detectRange = 4f;
    public float attackRange = 2f;
    private Transform currentTarget;
    private bool isFiring;

    public void Initialize(Ship shipComponent, GameObject projectile)
    {
        ship = shipComponent;
        movement = GetComponent<ShipMovement>();
        projectilePrefab = projectile;
    }

    public void Tick()
    {
        if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.position) > detectRange)
            FindTarget();

        if (currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.position);
            if (dist > attackRange)
            {
                movement.SetMoveTarget(currentTarget);
            }
            else
            {
                movement.SetMoveTarget(transform);
                if (!isFiring) StartCoroutine(FireAtTarget());
            }
        }
        else
        {
            movement.SetMoveTarget(ship.targetPlanet);
        }
    }

    void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRange);
        Transform closestTurret = null;
        Transform closestShip = null;
        float closestTurretDist = Mathf.Infinity;
        float closestShipDist = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<DefenseTurret>(out DefenseTurret turret))
            {
                if (turret.IsPlayerTurret() != ship.IsPlayer())
                {
                    float dist = Vector3.Distance(transform.position, turret.transform.position);
                    if (dist < closestTurretDist)
                    {
                        closestTurret = turret.transform;
                        closestTurretDist = dist;
                    }
                }
            }
            else if (hit.TryGetComponent<Ship>(out Ship otherShip))
            {
                if (otherShip.IsPlayer() != ship.IsPlayer())
                {
                    float dist = Vector3.Distance(transform.position, otherShip.transform.position);
                    if (dist < closestShipDist)
                    {
                        closestShip = otherShip.transform;
                        closestShipDist = dist;
                    }
                }
            }
        }

        currentTarget = closestTurret ?? closestShip;
    }

    IEnumerator FireAtTarget()
    {
        isFiring = true;

        while (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) <= attackRange)
        {
            ShootProjectile(currentTarget);
            yield return new WaitForSeconds(ship.fireCooldown);
        }

        isFiring = false;
    }

    void ShootProjectile(Transform target)
    {
        if (ship.GetShipType() == Ship.ShipType.Light)
        {
            if (target.TryGetComponent<Ship>(out Ship enemyShip))
            {
                enemyShip.TakeDamage(GetDamageAgainst(ship.GetShipType(), enemyShip.GetShipType()));
                DrawLaser(target.position);
            }
            else if (target.TryGetComponent<DefenseTurret>(out DefenseTurret turret))
            {
                turret.TakeDamage(GetDamageAgainst(ship.GetShipType(), Ship.ShipType.Heavy));
                DrawLaser(target.position);
            }
        }
        else
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            proj.GetComponent<Projectile>().SetTarget(target, ship.GetShipType(), ship.IsPlayer());
        }
    }

    float GetDamageAgainst(Ship.ShipType shooter, Ship.ShipType targetType)
    {
        if (shooter == Ship.ShipType.Light)
            return targetType == Ship.ShipType.Heavy ? 25 : 10;
        if (shooter == Ship.ShipType.Heavy)
            return targetType == Ship.ShipType.Drone ? 30 : 15;
        if (shooter == Ship.ShipType.Drone)
            return targetType == Ship.ShipType.Light ? 35 : 15;

        return 10;
    }

    void DrawLaser(Vector3 targetPos)
    {
        GameObject laser = new GameObject("Laser");
        var line = laser.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, targetPos);
        line.startWidth = line.endWidth = 0.05f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = ship.IsPlayer() ? Color.cyan : Color.red;
        line.endColor = ship.IsPlayer() ? Color.blue : Color.yellow;
        Destroy(laser, 0.1f);
    }
}

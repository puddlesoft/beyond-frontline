using UnityEngine;
using System;
using System.Collections;

public class ShipCombat : MonoBehaviour
{
    private Ship ship;
    private ShipMovement movement;
    private GameObject projectilePrefab;
    private ShipStateMachine stateMachine;


    public float detectRange = 4f;
    public float attackRange = 2f;
    private Transform currentTarget;
    private bool isFiring;
    private Transform lastCombatTarget;
    private bool wasInAttackRangeLastTick = false;

    public void Initialize(Ship shipComponent, GameObject projectile)
    {
        ship = shipComponent;
        movement = GetComponent<ShipMovement>();
        projectilePrefab = projectile;
        stateMachine = GetComponent<ShipStateMachine>();
    }

    public void Tick()
    {
        if (stateMachine != null && stateMachine.currentState != ShipState.Attacking)
        {
            return;
        }

        if (!IsTargetValid(currentTarget) || Vector3.Distance(transform.position, currentTarget.position) > detectRange)
        {
            FindTarget();
        }

        if (IsTargetValid(currentTarget))
        {
            float dist = Vector3.Distance(transform.position, currentTarget.position);

            if (currentTarget != lastCombatTarget)
            {
                Debug.Log($"[ShipCombat] {gameObject.name} acquired target: {currentTarget.name}");
                lastCombatTarget = currentTarget;
            }

            if (dist > attackRange)
            {
                wasInAttackRangeLastTick = false;
                movement.SetMoveTarget(currentTarget);
            }
            else
            {
                if (!wasInAttackRangeLastTick)
                {
                    Debug.Log($"[ShipCombat] {gameObject.name} is in attack range of {currentTarget.name}. Holding position to fire.");
                    wasInAttackRangeLastTick = true;
                }

                movement.SetMoveTarget(transform);

                if (!isFiring)
                {
                    StartCoroutine(FireAtTarget());
                }
            }
        }
        else
        {
            wasInAttackRangeLastTick = false;
            movement.SetMoveTarget(ship.targetPlanet);
        }

    }

    private bool IsTargetValid(Transform target)
    {
        return target != null && target.gameObject != null;
    }


    void FindTarget()
    {
        if (ship == null)
        {
            Debug.LogError($"[ShipCombat] Ship is NULL on {gameObject.name}, aborting FindTarget");
            return;
        }
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

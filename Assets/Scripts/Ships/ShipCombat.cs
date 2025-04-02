using UnityEngine;
using System;
using System.Collections;

public class ShipCombat : MonoBehaviour
{
    private Ship ship;
    private ShipMovement movement;
    private GameObject projectilePrefab;
    private ShipStateMachine stateMachine;

    public float detectRange = 5f;
    public float attackRange = 2f;  // Reduced from 3f to ensure ships get closer
    private Transform currentTarget;
    private bool isFiring;

    // Combat movement
    private float orbitAngle = 0f;
    private const float ORBIT_SPEED = 30f; // Reduced from 60f to 30f for slower orbit
    private const float ORBIT_RADIUS_MULTIPLIER = 0.8f; // How close to stay to attack range

    // Debug visualization
    public bool showDebug = true;
    private string debugText = "";
    private GUIStyle debugStyle;
    private bool isInitialized = false;

    void Awake()
    {
        // Initialize debug style immediately
        debugStyle = new GUIStyle();
        debugStyle.normal.textColor = Color.white; // Default color, will be updated in Initialize
        debugStyle.fontSize = 12;
        debugStyle.fontStyle = FontStyle.Bold;
        debugStyle.normal.background = Texture2D.grayTexture;
        debugStyle.padding = new RectOffset(5, 5, 5, 5);
    }

    public void Initialize(Ship shipComponent, GameObject projectile)
    {
        if (isInitialized)
        {
            Debug.LogWarning($"[ShipCombat] {gameObject.name} already initialized, skipping");
            return;
        }

        ship = shipComponent;
        movement = GetComponent<ShipMovement>();
        projectilePrefab = projectile;
        stateMachine = GetComponent<ShipStateMachine>();

        // Update debug style color based on ship ownership
        if (debugStyle != null)
        {
            debugStyle.normal.textColor = ship.IsPlayer() ? Color.cyan : Color.red;
        }
        else
        {
            Debug.LogError($"[ShipCombat] Debug style is null for {gameObject.name}!");
            return;
        }

        // Get initial debug state from GameManager
        var gameManager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
        if (gameManager != null)
        {
            showDebug = gameManager.showShipDebug;
        }
        else
        {
            Debug.LogWarning($"[ShipCombat] GameManager not found for {gameObject.name}, using default debug state");
        }

        // Set initial debug text
        debugText = $"State: Initializing\n" +
                   $"Type: {ship.GetShipType()}\n" +
                   $"Owner: {(ship.IsPlayer() ? "Player" : "Enemy")}";

        isInitialized = true;
        Debug.Log($"[ShipCombat] {gameObject.name} initialized as {(ship.IsPlayer() ? "Player" : "Enemy")} ship");
    }

    public void Tick()
    {
        if (!isInitialized || ship == null)
        {
            return;
        }

        // Always look for targets regardless of state
        bool hadTarget = IsTargetValid(currentTarget);
        if (!IsTargetValid(currentTarget) || Vector3.Distance(transform.position, currentTarget.position) > detectRange)
        {
            FindTarget();
        }

        // If we lost our target and didn't find a new one, notify state machine
        if (hadTarget && !IsTargetValid(currentTarget))
        {
            Debug.Log($"[ShipCombat] {gameObject.name} lost target, transitioning to Moving state");
            stateMachine.HandleTargetLost();
            orbitAngle = 0f;
            debugText = $"State: Moving\n" +
                       $"No Targets\n" +
                       $"To Planet: {ship.targetPlanet.name}";
        }

        // Update behavior based on current state
        switch (stateMachine.currentState)
        {
            case ShipState.Moving:
                if (IsTargetValid(currentTarget))
                {
                    float dist = Vector3.Distance(transform.position, currentTarget.position);
                    if (dist <= attackRange)
                    {
                        Debug.Log($"[ShipCombat] {gameObject.name} entering attack range ({dist:F1} <= {attackRange:F1}), transitioning to Attacking state");
                        stateMachine.HandleTargetAcquired(currentTarget);
                        orbitAngle = 0f;
                        debugText = $"State: Attacking\n" +
                                   $"Target: {currentTarget.name}\n" +
                                   $"Dist: {dist:F1}";
                    }
                    else
                    {
                        // Move toward target until in attack range
                        Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
                        Vector3 idealCombatPosition = currentTarget.position - (directionToTarget * (attackRange * 0.8f));
                        movement.SetMoveTarget(idealCombatPosition);
                        debugText = $"State: Moving\n" +
                                   $"Found Target: {currentTarget.name}\n" +
                                   $"Dist: {dist:F1}";
                    }
                }
                else
                {
                    debugText = $"State: Moving\n" +
                               $"To Planet: {ship.targetPlanet.name}\n" +
                               $"Dist: {Vector3.Distance(transform.position, ship.targetPlanet.position):F1}";
                }
                break;

            case ShipState.Attacking:
                if (IsTargetValid(currentTarget))
                {
                    float dist = Vector3.Distance(transform.position, currentTarget.position);
                    
                    if (dist <= attackRange)
                    {
                        UpdateCombatPosition();
                        
                        if (!isFiring)
                        {
                            StartCoroutine(FireAtTarget());
                        }

                        debugText = $"State: Attacking\n" +
                                   $"Target: {currentTarget.name}\n" +
                                   $"Dist: {dist:F1}\n" +
                                   $"Orbit: {orbitAngle:F1}°\n" +
                                   $"HP: {ship.currentHP}/{ship.maxHP}";
                    }
                    else
                    {
                        Debug.Log($"[ShipCombat] {gameObject.name} out of attack range ({dist:F1} > {attackRange:F1}), moving to maintain optimal distance");
                        // Move to maintain optimal combat distance
                        Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
                        Vector3 idealCombatPosition = currentTarget.position - (directionToTarget * (attackRange * 0.8f));
                        movement.SetMoveTarget(idealCombatPosition);
                        debugText = $"State: Moving\n" +
                                   $"Moving to Target: {currentTarget.name}\n" +
                                   $"Dist: {dist:F1}";
                    }
                }
                else
                {
                    // Look for new target before moving to planet
                    FindTarget();
                    if (!IsTargetValid(currentTarget))
                    {
                        Debug.Log($"[ShipCombat] {gameObject.name} no valid targets, transitioning to Moving state");
                        stateMachine.HandleTargetLost();
                        orbitAngle = 0f;
                        debugText = $"State: Moving\n" +
                                   $"No Targets\n" +
                                   $"To Planet: {ship.targetPlanet.name}";
                    }
                }
                break;

            case ShipState.Dead:
                debugText = $"State: Dead\n" +
                           $"HP: 0/{ship.maxHP}";
                break;
        }
    }

    private void UpdateCombatPosition()
    {
        if (!IsTargetValid(currentTarget)) return;

        // Get the other ship's combat component
        ShipCombat targetCombat = currentTarget.GetComponent<ShipCombat>();
        if (targetCombat == null) return;

        // Calculate the midpoint between the two ships
        Vector3 midpoint = (transform.position + currentTarget.position) * 0.5f;
        
        // Calculate the orbit radius (half of the combined attack ranges)
        float orbitRadius = (attackRange + targetCombat.attackRange) * 0.8f;
        
        // Determine orbit direction based on ship ownership (player ships go clockwise, enemy ships counter-clockwise)
        float orbitDirection = ship.IsPlayer() ? 1f : -1f;
        
        // Update orbit angle with smaller step size
        orbitAngle += ORBIT_SPEED * orbitDirection * Time.deltaTime * 0.2f;
        if (orbitAngle >= 360f) orbitAngle -= 360f;
        if (orbitAngle < 0f) orbitAngle += 360f;

        // Calculate position on orbit
        Vector3 orbitPosition = midpoint + new Vector3(
            Mathf.Cos(orbitAngle * Mathf.Deg2Rad) * orbitRadius,
            Mathf.Sin(orbitAngle * Mathf.Deg2Rad) * orbitRadius,
            0
        );

        // Calculate the direction to the orbit position
        Vector3 directionToOrbit = (orbitPosition - transform.position).normalized;
        
        // Set the move target to the orbit position
        movement.SetMoveTarget(orbitPosition);
        
        // Update debug text
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        debugText = $"State: Attacking\n" +
                   $"Target: {currentTarget.name}\n" +
                   $"Orbit Angle: {orbitAngle:F1}°\n" +
                   $"Orbit Radius: {orbitRadius:F1}\n" +
                   $"Distance: {distanceToTarget:F2}\n" +
                   $"Direction: {(orbitDirection > 0 ? "Clockwise" : "Counter")}";

        // Draw debug lines in play mode
        if (Application.isPlaying)
        {
            // Draw orbit circle
            int segments = 32;
            Vector3 previousPoint = midpoint;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i * 360f / segments) * Mathf.Deg2Rad;
                Vector3 orbitPoint = midpoint + new Vector3(
                    Mathf.Cos(angle) * orbitRadius,
                    Mathf.Sin(angle) * orbitRadius,
                    0
                );
                Debug.DrawLine(previousPoint, orbitPoint, Color.cyan, 0.1f);
                previousPoint = orbitPoint;
            }

            // Draw current position and target
            Debug.DrawLine(transform.position, orbitPosition, Color.yellow, 0.1f);
            Debug.DrawLine(orbitPosition, currentTarget.position, Color.green, 0.1f);
            
            // Draw orbit direction
            Debug.DrawRay(transform.position, directionToOrbit * orbitRadius, Color.magenta, 0.1f);
        }
    }

    public void ClearCombatState()
    {
        StopAllCoroutines();
        currentTarget = null;
        isFiring = false;
        orbitAngle = 0f;
        // Ensure we're moving toward the planet when combat ends
        if (ship != null && ship.targetPlanet != null)
        {
            movement.SetMoveTarget(ship.targetPlanet);
        }
    }

    private void OnGUI()
    {
        if (!showDebug || !isInitialized) return;

        // Calculate screen position for the ship
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPos.z < 0) return; // Don't draw if behind camera

        // Convert to GUI coordinates
        screenPos.y = Screen.height - screenPos.y;

        // Draw debug text
        GUI.Label(new Rect(screenPos.x + 20, screenPos.y - 20, 200, 100), debugText, debugStyle);
    }

    private void OnDrawGizmos()
    {
        // Always draw debug visualization in editor
        if (!Application.isPlaying) return;

        // Draw detection range
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Semi-transparent yellow
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // Draw attack range
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Semi-transparent red
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw orbit path if we have a target
        if (IsTargetValid(currentTarget))
        {
            ShipCombat targetCombat = currentTarget.GetComponent<ShipCombat>();
            if (targetCombat != null)
            {
                // Calculate midpoint and orbit radius
                Vector3 midpoint = (transform.position + currentTarget.position) * 0.5f;
                float orbitRadius = (attackRange + targetCombat.attackRange) * 0.4f;

                // Draw orbit circle with thicker lines
                Gizmos.color = new Color(0f, 1f, 1f, 0.5f); // Semi-transparent cyan
                int segments = 32;
                Vector3 previousPoint = midpoint;

                for (int i = 0; i <= segments; i++)
                {
                    float angle = (i * 360f / segments) * Mathf.Deg2Rad;
                    Vector3 orbitPoint = midpoint + new Vector3(
                        Mathf.Cos(angle) * orbitRadius,
                        Mathf.Sin(angle) * orbitRadius,
                        0
                    );

                    // Draw thicker line
                    Gizmos.DrawLine(previousPoint, orbitPoint);
                    Gizmos.DrawLine(previousPoint + Vector3.forward * 0.1f, orbitPoint + Vector3.forward * 0.1f);
                    previousPoint = orbitPoint;
                }

                // Draw line to target
                Gizmos.color = new Color(0f, 1f, 0f, 0.5f); // Semi-transparent green
                Gizmos.DrawLine(transform.position, currentTarget.position);
                Gizmos.DrawLine(transform.position + Vector3.forward * 0.1f, currentTarget.position + Vector3.forward * 0.1f);
            }
        }
    }

    private bool IsTargetValid(Transform target)
    {
        if (target == null || target.gameObject == null) return false;
        
        // Check if target is still valid (hasn't been destroyed)
        if (target.TryGetComponent<Ship>(out Ship targetShip))
        {
            return targetShip != null && targetShip.IsPlayer() != ship.IsPlayer();
        }
        else if (target.TryGetComponent<DefenseTurret>(out DefenseTurret turret))
        {
            return turret != null && turret.IsPlayerTurret() != ship.IsPlayer();
        }
        return false;
    }

    private void FindTarget()
    {
        if (!isInitialized || ship == null)
        {
            Debug.LogError($"[ShipCombat] Ship is NULL on {gameObject.name}, aborting FindTarget");
            return;
        }

        // Find all ships in the scene
        var allShips = UnityEngine.Object.FindObjectsByType<Ship>(FindObjectsSortMode.None);
        Transform nearest = null;
        float nearestDist = detectRange;

        foreach (var otherShip in allShips)
        {
            if (otherShip == null || otherShip.gameObject == gameObject) continue;
            
            // Check if this is an enemy ship
            if (otherShip.IsPlayer() != ship.IsPlayer())
            {
                float dist = Vector3.Distance(transform.position, otherShip.transform.position);
                if (dist < nearestDist)
                {
                    nearest = otherShip.transform;
                    nearestDist = dist;
                }
            }
        }

        if (nearest != null)
        {
            currentTarget = nearest;
            // Only enter attacking state if we're in attack range
            if (nearestDist <= attackRange)
            {
                stateMachine.HandleTargetAcquired(nearest);
                debugText = $"State: Attacking\n" +
                           $"Target: {nearest.name}\n" +
                           $"Dist: {nearestDist:F1}";
            }
            else
            {
                // Force movement state when target is found but out of range
                stateMachine.HandleTargetLost();
                debugText = $"State: Moving\n" +
                           $"Found Target: {nearest.name}\n" +
                           $"Dist: {nearestDist:F1}";
            }
        }
        else
        {
            currentTarget = null;
            debugText = $"State: Moving\n" +
                       $"No Target Found\n" +
                       $"To Planet: {ship.targetPlanet.name}";
        }
    }

    IEnumerator FireAtTarget()
    {
        if (stateMachine.currentState != ShipState.Attacking) 
        {
            Debug.LogWarning($"[ShipCombat] {gameObject.name} tried to fire while not in Attacking state");
            yield break;
        }
        
        isFiring = true;

        while (IsTargetValid(currentTarget) && stateMachine.currentState == ShipState.Attacking)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.position);
            
            if (dist <= attackRange)
            {
                ShootProjectile(currentTarget);
                yield return new WaitForSeconds(ship.fireCooldown);
            }
            else
            {
                // Move to maintain optimal combat distance
                Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
                Vector3 idealCombatPosition = currentTarget.position - (directionToTarget * (attackRange * 0.8f));
                movement.SetMoveTarget(idealCombatPosition);
                yield return new WaitForSeconds(0.2f);
            }
        }

        isFiring = false;
        
        if (stateMachine.currentState == ShipState.Attacking && !IsTargetValid(currentTarget))
        {
            stateMachine.HandleTargetLost();
        }
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

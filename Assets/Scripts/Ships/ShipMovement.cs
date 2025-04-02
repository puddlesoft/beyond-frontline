using UnityEngine;
using System;

public class ShipMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    private Transform targetPlanet;
    private Transform moveTarget;
    private Vector3? moveTargetPosition;
    private Transform lastMoveTarget;
    private ShipStateMachine stateMachine;
    private Vector3 currentVelocity;
    private const float SMOOTHING_TIME = 0.3f;

    public void Initialize(Transform target)
    {
        targetPlanet = target;
        moveTarget = target;
        stateMachine = GetComponent<ShipStateMachine>();

        if (targetPlanet == null)
        {
            Debug.LogError($"[ShipMovement] No targetPlanet set for {gameObject.name}");
        }
    }

    public void Tick()
    {
        if (stateMachine == null || stateMachine.currentState == ShipState.Dead)
            return;

        if (moveTarget == null && !moveTargetPosition.HasValue)
        {
            Debug.LogWarning($"[ShipMovement] No moveTarget set on {gameObject.name}");
            return;
        }

        // Handle movement based on state
        switch (stateMachine.currentState)
        {
            case ShipState.Moving:
                Vector3 targetPosition = moveTargetPosition ?? moveTarget.position;
                float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
                
                // Use SmoothDamp for smooth movement
                Vector3 oldPosition = transform.position;
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    targetPosition,
                    ref currentVelocity,
                    SMOOTHING_TIME,
                    moveSpeed
                );

                Debug.Log($"[ShipMovement] {gameObject.name} Moving: {oldPosition} -> {transform.position} (dist: {distanceToTarget:F2})");

                // Debug movement
                Vector3 direction = (targetPosition - transform.position).normalized;
                Debug.DrawRay(transform.position, direction * moveSpeed, Color.green);
                break;

            case ShipState.Attacking:
                targetPosition = moveTargetPosition ?? moveTarget.position;
                distanceToTarget = Vector3.Distance(transform.position, targetPosition);
                
                // Get the combat component to check attack range
                ShipCombat combat = GetComponent<ShipCombat>();
                if (combat == null) break;

                // Move directly to the orbit position with reduced speed
                Vector3 previousPosition = transform.position;
                
                // Calculate direction to target and perpendicular direction for orbital movement
                Vector3 directionToTarget = (targetPosition - transform.position).normalized;
                Vector3 perpendicularDirection = new Vector3(-directionToTarget.y, directionToTarget.x, 0);
                
                // Combine forward and orbital movement
                Vector3 moveDirection = (directionToTarget + perpendicularDirection * 0.5f).normalized;
                float moveDistance = moveSpeed * 0.1f * Time.deltaTime;
                transform.position += moveDirection * moveDistance;

                Debug.Log($"[ShipMovement] {gameObject.name} Attacking: {previousPosition} -> {transform.position} (dist: {distanceToTarget:F2})");

                // Debug movement - always show these rays
                Debug.DrawRay(transform.position, moveDirection * moveSpeed * 0.1f, Color.red);
                Debug.DrawRay(transform.position, perpendicularDirection * moveSpeed * 0.1f, Color.blue);
                break;

            case ShipState.Idle:
                // Do nothing while idle
                break;
        }
    }

    public void SetMoveTarget(Transform target)
    {
        if (target == null)
        {
            Debug.LogError($"[ShipMovement] Attempted to set null moveTarget on {gameObject.name}");
            return;
        }

        if (target == moveTarget) return; // No change → skip

        moveTarget = target;
        moveTargetPosition = null;
    }

    public void SetMoveTarget(Vector3 position)
    {
        Debug.Log($"[ShipMovement] {gameObject.name} setting move target position: {position}");
        moveTarget = null;
        moveTargetPosition = position;
    }

    public Transform GetCurrentTarget() => moveTarget;
    public Transform GetTargetPlanet() => targetPlanet;
}

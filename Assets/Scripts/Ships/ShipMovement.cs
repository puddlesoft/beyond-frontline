using UnityEngine;
using System;

public class ShipMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    private Transform targetPlanet;
    private Transform moveTarget;
    private Transform lastMoveTarget;
    private ShipStateMachine stateMachine;


    public void Initialize(Transform target)
    {
        targetPlanet = target;
        moveTarget = target;
        stateMachine = GetComponent<ShipStateMachine>();

        if (targetPlanet == null)
        {
            Debug.LogError($"[ShipMovement] No targetPlanet set for {gameObject.name}");
        }
        else
        {
            Debug.Log($"[ShipMovement] Initialized with targetPlanet: {targetPlanet.name} on {gameObject.name}");
        }
    }

    public void Tick()
    {
        if (stateMachine == null || stateMachine.currentState == ShipState.Moving)
        {
            if (moveTarget == null)
            {
                Debug.LogWarning($"[ShipMovement] No moveTarget set on {gameObject.name}");
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, moveTarget.position, moveSpeed * Time.deltaTime);
        }
    }



    public void SetMoveTarget(Transform target)
    {
        if (target == moveTarget) return; // No change → skip

        moveTarget = target;

        // Only log when the target changes
        if (target != lastMoveTarget)
        {
            Debug.Log($"[ShipMovement] moveTarget set to {moveTarget.name} on {gameObject.name}");
            lastMoveTarget = moveTarget;
        }
    }



    public Transform GetCurrentTarget() => moveTarget;
}

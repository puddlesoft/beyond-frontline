using UnityEngine;

public enum ShipState
{
    Idle,
    Moving,
    Attacking,
    Dead
}

public class ShipStateMachine : MonoBehaviour
{
    public ShipState currentState { get; private set; }
    
    private Ship ship;
    private ShipMovement movement;
    private ShipCombat combat;

    private void Awake()
    {
        currentState = ShipState.Idle;
    }

    public void Initialize()
    {
        ship = GetComponent<Ship>();
        movement = GetComponent<ShipMovement>();
        combat = GetComponent<ShipCombat>();

        if (ship == null || movement == null || combat == null)
        {
            Debug.LogError($"[FSM] {gameObject.name} missing required components during initialization");
        }
    }

    public void SetState(ShipState newState)
    {
        if (currentState == newState) return;

        // Exit current state
        switch (currentState)
        {
            case ShipState.Attacking:
                ExitAttackingState();
                break;
        }

        var oldState = currentState;
        currentState = newState;

        // Enter new state
        switch (newState)
        {
            case ShipState.Moving:
                EnterMovingState();
                break;
            case ShipState.Attacking:
                EnterAttackingState();
                break;
            case ShipState.Dead:
                EnterDeadState();
                break;
        }
    }

    private void EnterMovingState()
    {
        if (movement != null && ship != null && ship.targetPlanet != null)
        {
            movement.SetMoveTarget(ship.targetPlanet);
        }
        else
        {
            Debug.LogError($"[FSM] {gameObject.name} missing required components for Moving state");
        }
    }

    private void EnterAttackingState()
    {
        if (combat == null)
        {
            Debug.LogError($"[FSM] {gameObject.name} missing ShipCombat component");
        }
    }

    private void ExitAttackingState()
    {
        if (combat != null)
        {
            combat.ClearCombatState();
        }
        // Resume moving toward target planet when exiting attack state
        if (movement != null && ship != null && ship.targetPlanet != null)
        {
            movement.SetMoveTarget(ship.targetPlanet);
        }
    }

    private void EnterDeadState()
    {
        // Any cleanup or death effects would go here
    }

    public void HandleTargetAcquired(Transform target)
    {
        if (currentState == ShipState.Dead) return;
        SetState(ShipState.Attacking);
    }

    public void HandleTargetLost()
    {
        if (currentState == ShipState.Dead) return;
        SetState(ShipState.Moving);
    }
}

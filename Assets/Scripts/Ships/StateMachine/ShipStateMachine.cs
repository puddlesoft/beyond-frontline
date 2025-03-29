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

    public void SetState(ShipState newState)
    {
        if (currentState == newState) return;

        Debug.Log($"[FSM] {gameObject.name} state changed: {currentState} -> {newState}");
        currentState = newState;
    }

    private void Awake()
    {
        currentState = ShipState.Idle;
    }
}

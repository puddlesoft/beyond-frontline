using UnityEngine;

public class ShipMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    private Transform targetPlanet;
    private Transform moveTarget;

    private void Start()
    {
        moveTarget = targetPlanet;
    }

    public void Initialize(Transform target)
    {
        targetPlanet = target;
        moveTarget = targetPlanet;
    }

    public void Tick()
    {
        if (moveTarget != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveTarget.position, moveSpeed * Time.deltaTime);
        }
    }

    public void SetMoveTarget(Transform newTarget)
    {
        moveTarget = newTarget;
    }

    public Transform GetCurrentTarget() => moveTarget;
}

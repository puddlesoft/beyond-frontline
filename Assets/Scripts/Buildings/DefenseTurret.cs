using UnityEngine;

public class DefenseTurret : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float fireCooldown = 2f;
    public float range = 3.5f;
    public LayerMask targetLayerMask;
    public float health = 100f;

    public bool isPlayerTurret;

    private float fireTimer = 0f;
    private Transform currentTarget;

    void Update()
    {
        fireTimer -= Time.deltaTime;

        if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.position) > range)
        {
            FindTarget();
        }

        if (currentTarget != null && fireTimer <= 0f)
        {
            FireAtTarget();
            fireTimer = fireCooldown;
        }
    }

    void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, targetLayerMask);

        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (Collider2D hit in hits)
        {
            Ship ship = hit.GetComponent<Ship>();
            if (ship != null && ship.isPlayerShip != isPlayerTurret) // Only target enemies
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = ship.transform;
                }
            }
        }

        currentTarget = closest;
    }

    void FireAtTarget()
    {
        if (projectilePrefab != null && currentTarget != null)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Projectile projectile = proj.GetComponent<Projectile>();
            projectile.SetTarget(currentTarget, Ship.ShipType.Heavy); // Can be more dynamic later
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

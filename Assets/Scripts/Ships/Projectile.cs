using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public float damage;
    private Transform target;

    private Ship.ShipType shooterType;
    private bool isPlayerProjectile;

    public void SetTarget(Transform targetTransform, Ship.ShipType type, bool isPlayer)
    {
        target = targetTransform;
        shooterType = type;
        isPlayerProjectile = isPlayer;

        Ship targetShip = target?.GetComponent<Ship>();
        if (targetShip != null)
        {
            damage = GetDamageAgainst(shooterType, targetShip.shipType);
        }

        TintBasedOnOwnership();
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            Ship ship = target.GetComponent<Ship>();
            if (ship != null)
            {
                ship.TakeDamage(damage);
            }
            Destroy(gameObject);
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

    void TintBasedOnOwnership()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.color = isPlayerProjectile ? Color.cyan : Color.red;
    }
}

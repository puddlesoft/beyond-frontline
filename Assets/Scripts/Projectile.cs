using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public float damage;
    private Transform target;

    public void SetTarget(Transform tgt, Ship.ShipType shooterType)
    {
        target = tgt;
        damage = GetDamageAgainst(shooterType, tgt.GetComponent<Ship>().shipType);
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
            target.GetComponent<Ship>().TakeDamage(damage);
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
}

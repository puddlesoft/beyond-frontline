using UnityEngine;

public class Shipyard : MonoBehaviour
{
    public enum ShipyardType { Light, Heavy, Drone }
    public ShipyardType shipyardType;

    public float buildInterval = 5f;
    private float buildTimer;

    [Header("Spawn Settings")]
    public GameObject shipPrefab;
    public Transform targetPlanet;

    private PlayerResourceSystem playerSystem;

    public void Initialize(PlayerResourceSystem system, Transform enemyPlanet)
    {
        playerSystem = system;
        targetPlanet = enemyPlanet;
        buildTimer = buildInterval;

        playerSystem.RegisterShipyard(shipyardType);
    }



    void Update()
    {
        if (playerSystem == null) return;

        buildTimer -= Time.deltaTime;
        if (buildTimer <= 0)
        {
            if (playerSystem.CanAffordShip(shipyardType))
            {
                playerSystem.PayForShip(shipyardType);
                SpawnShip();
                buildTimer = buildInterval;
            }
        }
    }

    void SpawnShip()
    {
        GameObject ship = Instantiate(shipPrefab, transform.position, Quaternion.identity);
        ship.GetComponent<Ship>().SetupShip(targetPlanet, true, playerSystem);
        playerSystem.IncrementShipCount();
    }
}

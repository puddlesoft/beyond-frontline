using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    public GameObject lightShipyardPrefab;
    public GameObject heavyShipyardPrefab;
    public GameObject defenseTurretPrefab;

    public Transform playerPlanet;
    public Transform enemyPlanet;

    public PlayerResourceSystem playerSystem;
    public EnemyResourceSystem enemySystem;

    void Start()
    {
        PlaceInitialStructures(playerPlanet.position, playerSystem, true);
        PlaceInitialStructures(enemyPlanet.position, enemySystem, false);
    }

    void PlaceInitialStructures(Vector3 center, object ownerSystem, bool isPlayer)
    {
        float shipyardDistance = 1.5f;

        Vector3[] positions = {
            center + new Vector3(shipyardDistance, 0),
            center + new Vector3(-shipyardDistance, 0),
            center + new Vector3(0, shipyardDistance),
        };

        foreach (var pos in positions)
        {
            GameObject shipyard = Instantiate(lightShipyardPrefab, pos, Quaternion.identity);
            if (isPlayer)
            {
                shipyard.GetComponent<Shipyard>().Initialize((PlayerResourceSystem)ownerSystem, ((PlayerResourceSystem)ownerSystem).enemyPlanet);
            }
            else
            {
                shipyard.GetComponent<Shipyard>().Initialize(null, ((EnemyResourceSystem)ownerSystem).targetPlanet, false, (EnemyResourceSystem)ownerSystem);
            }
        }

        Vector2 randomOffset = Random.insideUnitCircle.normalized * Random.Range(2.5f, 3.5f);
        Vector3 defensePos = center + new Vector3(randomOffset.x, randomOffset.y);

        GameObject turret = Instantiate(defenseTurretPrefab, defensePos, Quaternion.identity);
        turret.GetComponent<DefenseTurret>().isPlayerTurret = isPlayer;
    }
}

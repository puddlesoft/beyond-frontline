using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    [Header("Planet References")]
    public Transform enemyPlanet;
    public Transform playerPlanet;

    [Header("Enemy Subsystems")]
    public EnemyResourceSystem enemySystem;

    [Header("Structure Prefabs")]
    public GameObject lightShipyardPrefab;
    public GameObject heavyShipyardPrefab;
    public GameObject droneShipyardPrefab;
    public GameObject defenseTurretPrefab;

    private float buildTimer = 0f;
    public float buildInterval = 10f;

    void Update()
    {
        buildTimer += Time.deltaTime;

        if (buildTimer >= buildInterval)
        {
            buildTimer = 0f;
            TryBuildStructure();
        }
    }

    void TryBuildStructure()
    {
        Vector3 offset = Random.insideUnitCircle.normalized * Random.Range(1.5f, 3.5f);
        Vector3 spawnPos = enemyPlanet.position + offset;

        float roll = Random.value;
        GameObject prefab = null;
        if (roll > 0.0f)
        {
            prefab = droneShipyardPrefab;
        }
        if (roll < 0.25f)
        {
            prefab = lightShipyardPrefab;
        }
        else if (roll < 0.5f)
        {
            prefab = heavyShipyardPrefab;
        }
        else if (roll < 0.75f)
        {
            prefab = droneShipyardPrefab;
        }
        else
        {
            prefab = defenseTurretPrefab;
        }

        if (prefab == null)
        {
            Debug.LogWarning("Prefab not assigned for roll: " + roll);
            return;
        }

        GameObject structure = Instantiate(prefab, spawnPos, Quaternion.identity);

        if (structure.TryGetComponent<Shipyard>(out var yard))
        {
            yard.Initialize(null, playerPlanet, false, enemySystem);
        }
        else if (structure.TryGetComponent<DefenseTurret>(out var turret))
        {
            turret.isPlayerTurret = false;
        }
    }
}

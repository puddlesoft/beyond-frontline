using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    public GameObject lightShipyardPrefab;
    public GameObject defenseTurretPrefab;
    public Transform enemyPlanet;
    public EnemyResourceSystem enemySystem;

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

        GameObject structure;
        if (Random.value > 0.5f)
        {
            structure = Instantiate(lightShipyardPrefab, spawnPos, Quaternion.identity);
            structure.GetComponent<Shipyard>().Initialize(null, enemySystem.targetPlanet, false, enemySystem);
        }
        else
        {
            structure = Instantiate(defenseTurretPrefab, spawnPos, Quaternion.identity);
            structure.GetComponent<DefenseTurret>().isPlayerTurret = false;
        }
    }
}

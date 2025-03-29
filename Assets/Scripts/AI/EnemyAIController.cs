using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
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

        float roll = Random.value;
        GameObject prefab = null;

        if (roll < 0.25f)
        {
            prefab = Resources.Load<GameObject>("Buildings/LightShipyard");
        }
        else if (roll < 0.5f)
        {
            prefab = Resources.Load<GameObject>("Buildings/HeavyShipyard");
        }
        else if (roll < 0.75f)
        {
            prefab = Resources.Load<GameObject>("Buildings/DroneShipyard");
        }
        else
        {
            prefab = Resources.Load<GameObject>("Buildings/DefenseTurret");
        }

        if (prefab == null)
        {
            Debug.LogWarning("Prefab not found for roll: " + roll);
            return;
        }

        GameObject structure = Instantiate(prefab, spawnPos, Quaternion.identity);

        if (structure.TryGetComponent<Shipyard>(out var yard))
        {
            yard.Initialize(null, enemySystem.GetTargetPlanet(), false, enemySystem);
        }
        else if (structure.TryGetComponent<DefenseTurret>(out var turret))
        {
            turret.isPlayerTurret = false;
        }
    }
}

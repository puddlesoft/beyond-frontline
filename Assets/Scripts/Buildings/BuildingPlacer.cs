using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    public Transform planetPlayer;
    public float orbitMin = 2f;
    public float orbitMax = 4f;

    private GameObject preview;
    private GameObject currentPrefab;
    private bool placing;

    public PlayerResourceSystem playerSystem;

    void Update()
    {
        if (!placing || currentPrefab == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        preview.transform.position = mouseWorld;

        if (Input.GetMouseButtonDown(0))
        {
            float dist = Vector3.Distance(mouseWorld, planetPlayer.position);
            if (dist >= orbitMin && dist <= orbitMax)
            {
                GameObject final = Instantiate(currentPrefab, mouseWorld, Quaternion.identity);
                var shipyard = final.GetComponent<Shipyard>();
                shipyard.Initialize(playerSystem, playerSystem.enemyPlanet);
                placing = false;
                Destroy(preview);
            }
            else
            {
                Debug.Log("Must place between 2–4 units from planet.");
            }
        }
    }

    public void StartPlacing(GameObject prefab)
    {
        if (placing && preview != null)
        {
            Destroy(preview);
        }

        currentPrefab = prefab;
        preview = Instantiate(prefab);
        placing = true;
    }
}

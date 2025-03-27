using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacer : MonoBehaviour
{
    public enum BuildingType { Shipyard, Defense }

    public GameObject ghostPrefab;
    public Transform planet;
    public Material validMaterial;
    public Material invalidMaterial;

    private float minRange;
    private float maxRange;
    private bool keepPlacing = false;

    private GameObject ghostInstance;
    private BuildingType currentType;
    private GameObject actualBuildingPrefab;
    private PlayerResourceSystem playerSystem;
    private LineRenderer ringRenderer;

    void Update()
    {
        if (ghostInstance == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        ghostInstance.transform.position = mousePos;

        float distance = Vector3.Distance(mousePos, planet.position);
        bool valid = distance >= minRange && distance <= maxRange && !EventSystem.current.IsPointerOverGameObject();

        SpriteRenderer ghostSprite = ghostInstance.GetComponent<SpriteRenderer>();
        ghostSprite.material = valid ? validMaterial : invalidMaterial;

        if (Input.GetMouseButtonDown(0) && valid)
        {
            GameObject built = Instantiate(actualBuildingPrefab, mousePos, Quaternion.identity);

            if (currentType == BuildingType.Shipyard)
            {
                Shipyard yard = built.GetComponent<Shipyard>();
                yard.Initialize(playerSystem, playerSystem.enemyPlanet);
            }

            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (!shiftHeld)
            {
                Destroy(ghostInstance);
                Destroy(ringRenderer.gameObject);
            }
            else
            {
                ghostInstance.transform.position = Vector3.zero; // reset for clean placement
            }
        }

        // Cancel placement when Shift is released and mouse button is up
        if ((Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift)) && ghostInstance != null)
        {
            if (!Input.GetMouseButton(0)) // avoid canceling mid-click
            {
                Destroy(ghostInstance);
                Destroy(ringRenderer.gameObject);
            }
        }


    }

    public void StartPlacing(GameObject prefab, PlayerResourceSystem player, BuildingType type)
    {
        currentType = type;
        actualBuildingPrefab = prefab;
        playerSystem = player;

        minRange = type == BuildingType.Shipyard ? 0.25f : 2.5f;
        maxRange = type == BuildingType.Shipyard ? 2f : 4.5f;

        ghostInstance = Instantiate(ghostPrefab);
        SpriteRenderer sourceRenderer = prefab.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer ghostRenderer = ghostInstance.GetComponent<SpriteRenderer>();

        if (sourceRenderer != null && ghostRenderer != null)
        {
            ghostRenderer.sprite = sourceRenderer.sprite;
        }

        ringRenderer = CreateRing(planet.position, maxRange);
    }

    LineRenderer CreateRing(Vector3 center, float radius)
    {
        GameObject ring = new GameObject("PlacementRing");
        LineRenderer lr = ring.AddComponent<LineRenderer>();
        lr.positionCount = 361;
        lr.loop = true;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = Color.white;

        for (int i = 0; i < 361; i++)
        {
            float rad = i * Mathf.Deg2Rad;
            lr.SetPosition(i, center + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * radius);
        }

        return lr;
    }
}

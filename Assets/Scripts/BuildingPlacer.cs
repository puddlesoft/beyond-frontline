using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    public GameObject buildingPreviewPrefab;
    GameObject currentPreview;

    public Transform planetPlayer;
    public ResourceManager resourceManager;

    Shipyard selectedShipyard;

    void Update()
    {
        if (currentPreview != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            currentPreview.transform.position = mousePos;

            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceBuilding(mousePos);
            }
        }
    }

    public void SelectBuildingToPlace(GameObject shipyardPrefab)
    {
        if (currentPreview != null)
            Destroy(currentPreview);

        currentPreview = Instantiate(shipyardPrefab);
        selectedShipyard = currentPreview.GetComponent<Shipyard>();
    }

    void TryPlaceBuilding(Vector3 position)
    {
        float distanceToPlanet = Vector3.Distance(position, planetPlayer.position);
        if (distanceToPlanet > 2f && distanceToPlanet < 4f) // distance range around planet
        {
            if (resourceManager.CanAffordBuilding(selectedShipyard))
            {
                resourceManager.PayForBuilding(selectedShipyard);
                currentPreview = null; // finalize placement
            }
            else
            {
                Debug.Log("Not enough resources to build!");
                Destroy(currentPreview);
            }
        }
        else
        {
            Debug.Log("Place shipyard within orbital range (2-4 units from planet)");
        }
    }
}

using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance;

    [Header("Prefabs")]
    public GameObject lightShipyardPrefab;
    public GameObject heavyShipyardPrefab;
    public GameObject droneShipyardPrefab;

    [Header("Dependencies")]
    public BuildingPlacer placer;

    void Awake()
    {
        Instance = this;
    }

    public void TryPlaceLight()
    {
        placer.StartPlacing(lightShipyardPrefab);
    }

    public void TryPlaceHeavy()
    {
        placer.StartPlacing(heavyShipyardPrefab);
    }

    public void TryPlaceDrone()
    {
        placer.StartPlacing(droneShipyardPrefab);
    }
}

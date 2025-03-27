using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance;

    [Header("Prefabs")]
    public GameObject lightShipyardPrefab;
    public GameObject heavyShipyardPrefab;
    public GameObject droneShipyardPrefab;
    public GameObject defenseStructurePrefab;


    [Header("Dependencies")]
    public BuildingPlacer placer;
    public PlayerResourceSystem playerSystem;

    void Awake()
    {
        Instance = this;
    }

    public void TryPlaceLight()
    {
        placer.StartPlacing(lightShipyardPrefab, playerSystem, BuildingPlacer.BuildingType.Shipyard);
    }

    public void TryPlaceHeavy()
    {
        placer.StartPlacing(heavyShipyardPrefab, playerSystem, BuildingPlacer.BuildingType.Shipyard);
    }

    public void TryPlaceDrone()
    {
        placer.StartPlacing(droneShipyardPrefab, playerSystem, BuildingPlacer.BuildingType.Shipyard);
    }

    public void TryPlaceDefense()
    {
        placer.StartPlacing(defenseStructurePrefab, playerSystem, BuildingPlacer.BuildingType.Defense);
    }

}

using UnityEngine;

public class PlayerResourceSystem : BaseResourceSystem
{
    public Transform enemyPlanet;
    public ShipProductionManager productionManager;

    public override Transform GetTargetPlanet() => enemyPlanet;

    public override void RegisterShipyard(Shipyard yard)
    {
        productionManager.RegisterShipyard(yard.GetShipyardType(), yard);
        CountShipyard(yard.GetShipyardType());

    }

    public override void EnqueueShipBuild(Shipyard yard)
    {
        productionManager.EnqueueBuild(yard.GetShipyardType(), yard);
    }

    public override void PayForShip(Shipyard.ShipyardType type)
    {
        switch (type)
        {
            case Shipyard.ShipyardType.Light:
                metalTubes -= 5;
                wiring -= 2;
                break;
            case Shipyard.ShipyardType.Heavy:
                metalTubes -= 10;
                circuits -= 5;
                break;
            case Shipyard.ShipyardType.Drone:
                wiring -= 4;
                circuits -= 4;
                break;
        }
    }

    public override bool CanAffordShip(Shipyard.ShipyardType type)
    {
        return type switch
        {
            Shipyard.ShipyardType.Light => metalTubes >= 5 && wiring >= 2,
            Shipyard.ShipyardType.Heavy => metalTubes >= 10 && circuits >= 5,
            Shipyard.ShipyardType.Drone => wiring >= 4 && circuits >= 4,
            _ => false
        };
    }
}

using UnityEngine;

public abstract class BaseResourceSystem : MonoBehaviour
{
    public float iron, energy, copper, silicon;
    public float ironRate = 10f, energyRate = 5f, copperRate = 3f, siliconRate = 2f;

    public int metalTubes, wiring, circuits;
    public int TotalShips { get; protected set; }

    protected int totalLightShipyards = 0;
    protected int totalHeavyShipyards = 0;
    protected int totalDroneShipyards = 0;


    public abstract Transform GetTargetPlanet();
    public abstract void RegisterShipyard(Shipyard yard);
    public abstract void EnqueueShipBuild(Shipyard yard);
    public abstract void PayForShip(Shipyard.ShipyardType type);
    public abstract bool CanAffordShip(Shipyard.ShipyardType type);

    public void Tick()
    {
        GenerateResources();
        ManufactureComponents();
    }

    protected virtual void GenerateResources()
    {
        iron += ironRate * Time.deltaTime;
        energy += energyRate * Time.deltaTime;
        copper += copperRate * Time.deltaTime;
        silicon += siliconRate * Time.deltaTime;
    }


    protected virtual void ManufactureComponents()
{
    if (iron >= 100 && energy >= 50) { iron -= 100; energy -= 50; metalTubes += 10;}
    if (copper >= 50 && silicon >= 20) { copper -= 50; silicon -= 20; wiring += 10;}
    if (iron >= 30 && copper >= 30 && silicon >= 40) { iron -= 30; copper -= 30; silicon -= 40; circuits += 5;}
}


    public void IncrementShipCount() => TotalShips++;
    public void DecrementShipCount() => TotalShips = Mathf.Max(0, TotalShips - 1);

    public virtual void GetRequiredRates(out float reqTubes, out float reqWiring, out float reqCircuits)
    {
        reqTubes = totalLightShipyards * 5 + totalHeavyShipyards * 10;
        reqWiring = totalLightShipyards * 2 + totalDroneShipyards * 4;
        reqCircuits = totalHeavyShipyards * 5 + totalDroneShipyards * 4;
    }

    public virtual float GetTubeRate()
    {
        return (iron >= 100 && energy >= 50) ? 10f / Time.deltaTime : 0f;
    }

    public virtual float GetWiringRate()
    {
        return (copper >= 50 && silicon >= 20) ? 10f / Time.deltaTime : 0f;
    }

    public virtual float GetCircuitRate()
    {
        return (iron >= 30 && copper >= 30 && silicon >= 40) ? 5f / Time.deltaTime : 0f;
    }

    protected void CountShipyard(Shipyard.ShipyardType type)
    {
        switch (type)
        {
            case Shipyard.ShipyardType.Light:
                totalLightShipyards++;
                break;
            case Shipyard.ShipyardType.Heavy:
                totalHeavyShipyards++;
                break;
            case Shipyard.ShipyardType.Drone:
                totalDroneShipyards++;
                break;
        }
    }


}

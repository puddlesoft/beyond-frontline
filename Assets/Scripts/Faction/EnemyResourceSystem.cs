using UnityEngine;

public class EnemyResourceSystem : MonoBehaviour
{
    public Transform targetPlanet;
    public float iron, energy, copper, silicon;
    public float ironRate = 10f, energyRate = 5f, copperRate = 3f, siliconRate = 2f;

    public int metalTubes, wiring, circuits;
    public int TotalShips { get; private set; }

    public void Tick()
    {
        GenerateResources();
        ManufactureComponents();
    }

    void GenerateResources()
    {
        iron += ironRate * Time.deltaTime;
        energy += energyRate * Time.deltaTime;
        copper += copperRate * Time.deltaTime;
        silicon += siliconRate * Time.deltaTime;
    }

    void ManufactureComponents()
    {
        if (iron >= 100 && energy >= 50) { iron -= 100; energy -= 50; metalTubes += 10; }
        if (copper >= 50 && silicon >= 20) { copper -= 50; silicon -= 20; wiring += 10; }
        if (iron >= 30 && copper >= 30 && silicon >= 40) { iron -= 30; copper -= 30; silicon -= 40; circuits += 5; }
    }

    public Transform GetTargetPlanet()
    {
        return targetPlanet;
    }


    public void IncrementShipCount() => TotalShips++;
    public void DecrementShipCount() => TotalShips = Mathf.Max(0, TotalShips - 1);
}

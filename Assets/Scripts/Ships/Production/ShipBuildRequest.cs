using UnityEngine;

public class ShipBuildRequest
{
    public Shipyard.ShipyardType type;
    public Shipyard origin;

    public ShipBuildRequest(Shipyard.ShipyardType type, Shipyard origin)
    {
        this.type = type;
        this.origin = origin;
    }
}

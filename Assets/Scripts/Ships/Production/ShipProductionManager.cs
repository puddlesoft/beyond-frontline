using System.Collections.Generic;
using UnityEngine;

public class ShipProductionManager : MonoBehaviour
{
    private Dictionary<Shipyard.ShipyardType, Queue<Shipyard>> shipyardQueues = new();
    private Queue<BuildRequest> buildQueue = new();
    private const int maxQueueSize = 50;

    public void RegisterShipyard(Shipyard.ShipyardType type, Shipyard yard)
    {
        if (!shipyardQueues.ContainsKey(type))
        {
            shipyardQueues[type] = new Queue<Shipyard>();
        }

        shipyardQueues[type].Enqueue(yard);
    }

    public void EnqueueBuild(Shipyard.ShipyardType type, Shipyard origin)
    {
        if (buildQueue.Count >= maxQueueSize)
            return;

        buildQueue.Enqueue(new BuildRequest(type, origin));
    }

    public void Tick(BaseResourceSystem player, BaseResourceSystem enemy)
    {
        ProcessBuildQueue(player, true);
        ProcessBuildQueue(enemy, false);
    }

    private void ProcessBuildQueue(BaseResourceSystem system, bool isPlayer)
    {
        if (system == null)
        {
            Debug.LogError($"[ShipProductionManager] {(isPlayer ? "Player" : "Enemy")} system is null!");
            return;
        }

        int buildsThisFrame = 0;

        int queueSize = buildQueue.Count;
        for (int i = 0; i < queueSize && buildsThisFrame < 3; i++) // Limit builds per frame
        {
            BuildRequest request = buildQueue.Dequeue();
            Shipyard yard = request.origin;

            if (yard == null) continue;

            bool canAfford = false;
            if (isPlayer && system is PlayerResourceSystem playerSystem)
            {
                canAfford = yard.CanPlayerAfford(playerSystem);
            }
            else if (!isPlayer && system is EnemyResourceSystem enemySystem)
            {
                canAfford = yard.CanEnemyAfford(enemySystem);
            }
            else
            {
                Debug.LogError($"[ShipProductionManager] Invalid system type for {(isPlayer ? "Player" : "Enemy")} shipyard!");
                continue;
            }

            if (canAfford)
            {
                yard.SpawnShip(yard.isPlayer);
                buildsThisFrame++;
            }
            else
            {
                // Re-enqueue for later
                buildQueue.Enqueue(request);
            }
        }
    }

    private class BuildRequest
    {
        public Shipyard.ShipyardType type;
        public Shipyard origin;

        public BuildRequest(Shipyard.ShipyardType type, Shipyard origin)
        {
            this.type = type;
            this.origin = origin;
        }
    }
}

using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Subsystems")]
    public PlayerResourceSystem playerSystem;
    public EnemyResourceSystem enemySystem;

    [Header("UI")]
    public TextMeshProUGUI playerShipCountText;
    public TextMeshProUGUI enemyShipCountText;
    public ShipProductionManager productionManager;

    [Header("Debug Settings")]
    public bool showShipDebug = true;  // Changed to true by default

    void Start()
    {
        // Find all existing ships and set their debug state
        UpdateAllShipDebugState();
    }

    void Update()
    {
        playerSystem.Tick();
        enemySystem.Tick();

        productionManager?.Tick(playerSystem, enemySystem);

        UpdateShipCounts();
    }

    void UpdateShipCounts()
    {
        if (playerShipCountText != null)
            playerShipCountText.text = $"Player Ships: {playerSystem.TotalShips}";
        if (enemyShipCountText != null)
            enemyShipCountText.text = $"Enemy Ships: {enemySystem.TotalShips}";
    }

    public void UpdateAllShipDebugState()
    {
        var allShips = UnityEngine.Object.FindObjectsByType<ShipCombat>(FindObjectsSortMode.None);
        foreach (var ship in allShips)
        {
            if (ship != null)
            {
                ship.showDebug = showShipDebug;
                Debug.Log($"[GameManager] Updated debug state for {ship.gameObject.name}: {showShipDebug}");
            }
        }
    }

    // Called by Unity when values are changed in the inspector
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateAllShipDebugState();
        }
    }
}

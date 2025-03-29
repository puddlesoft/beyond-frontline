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
}

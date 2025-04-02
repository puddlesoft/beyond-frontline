using UnityEngine;
using System.Collections.Generic;

public class Planet : MonoBehaviour
{
    [Header("Planet Settings")]
    public bool isPlayerPlanet = false;
    public float radius = 1f;
    public Color playerColor = Color.blue;
    public Color enemyColor = Color.red;
    
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D planetCollider;

    private void Start()
    {
        // Setup visual appearance
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Setup collider for ship detection
        planetCollider = GetComponent<CircleCollider2D>();
        if (planetCollider == null)
        {
            planetCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        planetCollider.radius = radius;
        planetCollider.isTrigger = true;
        
        UpdateVisuals();
    }

    public void SetOwnership(bool isPlayer)
    {
        isPlayerPlanet = isPlayer;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isPlayerPlanet ? playerColor : enemyColor;
        }
    }

    public bool IsPlayerPlanet()
    {
        return isPlayerPlanet;
    }

    public Transform GetTargetPlanet()
    {
        // Find the opposite faction's planet
        var planets = GameObject.FindGameObjectsWithTag("Planet");
        foreach (var planet in planets)
        {
            var planetComponent = planet.GetComponent<Planet>();
            if (planetComponent != null && planetComponent.IsPlayerPlanet() != isPlayerPlanet)
            {
                return planet.transform;
            }
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        // Draw planet radius
        Gizmos.color = isPlayerPlanet ? playerColor : enemyColor;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
} 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Attach this to any GameObject that should act as an obstacle.
/// It self-registers into the static list that Boid.cs checks each frame.
/// Also handles the cyberpunk neon visual for the obstacle.
public class ObstacleManager : MonoBehaviour
{
    // Static list all Boids read from
    static public List<Transform> obstacles = new List<Transform>();

    [Header("Cyberpunk Obstacle Visuals")]
    public Color neonColor = new Color(1f, 0f, 0.3f);
    public float pulseSpeed = 2f;
    public float pulseIntensity = 1.5f;

    private Renderer[] rends;
    private float timeOffset;

    void Awake()
    {
        // Register this obstacle
        if (!obstacles.Contains(transform))
            obstacles.Add(transform);

        // Make sure collider is on the Obstacle layer
        gameObject.layer = LayerMask.NameToLayer("Obstacle");

        rends = GetComponentsInChildren<Renderer>();
        timeOffset = Random.Range(0f, Mathf.PI * 2f);

        // Apply initial neon material look
        foreach (Renderer r in rends)
        {
            r.material.color = neonColor;
            r.material.SetColor("_EmissionColor", neonColor * pulseIntensity);
            r.material.EnableKeyword("_EMISSION");
        }
    }

    void OnDestroy()
    {
        // Unregister when destroyed
        if (obstacles.Contains(transform))
            obstacles.Remove(transform);
    }

    void Update()
    {
        // Pulsing neon glow effect
        float pulse = Mathf.Sin((Time.time + timeOffset) * pulseSpeed) * 0.5f + 0.5f;
        float emission = Mathf.Lerp(0.5f, pulseIntensity * 2f, pulse);

        foreach (Renderer r in rends)
        {
            r.material.SetColor("_EmissionColor", neonColor * emission);
        }
    }
}
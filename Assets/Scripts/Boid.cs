using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Set Dynamically")]
    public Rigidbody rigid; 

    private Neighborhood neighborhood;

    // Cyberpunk neon colors palette
    private static readonly Color[] neonColors = new Color[]
    {
        new Color(1f, 0f, 0.8f), 
        new Color(0f, 1f, 1f),  
        new Color(0.5f, 0f, 1f), 
        new Color(0f, 0.8f, 1f),
        new Color(1f, 0.9f, 0f), 
        new Color(0f, 1f, 0.4f), 
    };

    private Color boidColor;

    void Awake()
    {
        neighborhood = GetComponent<Neighborhood>();
        rigid = GetComponent<Rigidbody>();

        pos = Random.insideUnitSphere * Spawner.S.spawnRadius;

        Vector3 vel = Random.onUnitSphere * Spawner.S.velocity;
        rigid.velocity = vel;

        LookAhead();

        // Pick a random cyberpunk neon color
        boidColor = neonColors[Random.Range(0, neonColors.Length)];

        // Apply to all child renderers
        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            r.material.color = boidColor;
            // Enable emission for a glow effect
            r.material.SetColor("_EmissionColor", boidColor * 2f);
            r.material.EnableKeyword("_EMISSION");
        }

        // Set up the glowing neon gradient trail
        TrailRenderer tRend = GetComponent<TrailRenderer>();
        if (tRend != null)
        {
            SetupNeonTrail(tRend);
        }
    }

    void SetupNeonTrail(TrailRenderer tRend)
    {
        // Pick a complementary second color for the gradient
        Color trailEnd = new Color(boidColor.r * 0.2f, boidColor.g * 0.2f, boidColor.b * 0.2f, 0f);

        // Gradient: bright neon at the head, fades to transparent at the tail
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0.0f),
                new GradientColorKey(boidColor, 0.2f),
                new GradientColorKey(boidColor, 0.7f), 
                new GradientColorKey(trailEnd, 1.0f) 
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0.0f),
                new GradientAlphaKey(0.9f, 0.2f),
                new GradientAlphaKey(0.4f, 0.8f),
                new GradientAlphaKey(0f, 1.0f)
            }
        );

        tRend.colorGradient = gradient;
        tRend.startWidth = 0.4f;
        tRend.endWidth = 0.0f;
        tRend.time = 0.6f; 
        tRend.minVertexDistance = 0.1f;

        // Use the additive shader so trails glow and blend together (Unity 2022 path)
        Shader additiveShader = Shader.Find("Particles/Standard Unlit");
        if (additiveShader == null) additiveShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (additiveShader == null) additiveShader = Shader.Find("Standard");
        tRend.material = new Material(additiveShader);
        tRend.material.SetFloat("_Mode", 1);
    }

    void LookAhead()
    {
        transform.LookAt(pos + rigid.velocity);
    }

    public Vector3 pos
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    void FixedUpdate()
    {
        if (neighborhood == null || rigid == null) return;

        Vector3 vel = rigid.velocity;
        Spawner spn = Spawner.S;

        // --- Separation (collision avoidance between boids) ---
        Vector3 velAvoid = Vector3.zero;
        Vector3 tooClosePos = neighborhood.avgClosePos;

        if (tooClosePos != Vector3.zero)
        {
            velAvoid = pos - tooClosePos;
            velAvoid.Normalize();
            velAvoid *= spn.velocity;
        }

        // --- Alignment ---
        Vector3 velAlign = neighborhood.avgVel;
        if (velAlign != Vector3.zero)
        {
            velAlign.Normalize();
            velAlign *= spn.velocity;
        }

        // --- Cohesion ---
        Vector3 velCenter = neighborhood.avgPos;
        if (velCenter != Vector3.zero)
        {
            velCenter -= transform.position;
            velCenter.Normalize();
            velCenter *= spn.velocity;
        }

        // --- Attractor ---
        Vector3 delta = Attractor.POS - pos;
        bool attracted = (delta.magnitude > spn.attractPushDist);
        Vector3 velAttract = delta.normalized * spn.velocity;

        // --- Obstacle Avoidance ---
        Vector3 velObstacle = Vector3.zero;
        if (ObstacleManager.obstacles != null)
        {
            velObstacle = CalculateObstacleAvoidance(spn);
        }

        float fdt = Time.fixedDeltaTime;

        // Priority: boid separation > obstacle avoidance > alignment/cohesion/attraction
        if (velAvoid != Vector3.zero)
        {
            vel = Vector3.Lerp(vel, velAvoid, spn.collAvoid * fdt);
        }
        else if (velObstacle != Vector3.zero)
        {
            // Obstacle avoidance blends in strongly
            vel = Vector3.Lerp(vel, velObstacle, spn.obstacleAvoid * fdt);
        }
        else
        {
            if (velAlign != Vector3.zero)
                vel = Vector3.Lerp(vel, velAlign, spn.velMatching * fdt);

            if (velCenter != Vector3.zero)
                vel = Vector3.Lerp(vel, velCenter, spn.flockCentering * fdt);

            if (velAttract != Vector3.zero)
            {
                if (attracted)
                    vel = Vector3.Lerp(vel, velAttract, spn.attractPull * fdt);
                else
                    vel = Vector3.Lerp(vel, -velAttract, spn.attractPush * fdt);
            }
        }

        vel = vel.normalized * spn.velocity;
        rigid.velocity = vel;
        LookAhead();
    }

    /// Shoots two parallel rays (spanning the boid's width) in the direction of motion.
    /// If either hits an obstacle, steers away from it.
    Vector3 CalculateObstacleAvoidance(Spawner spn)
    {
        Vector3 avoidVel = Vector3.zero;
        Vector3 forward = rigid.velocity.normalized;
        float detectionRange = spn.obstacleDetectDist;

        // Two parallel rays offset left/right of the boid's width
        float boidHalfWidth = 1.0f;
        Vector3 right = Vector3.Cross(forward, Vector3.up).normalized * boidHalfWidth;
        Vector3[] rayOrigins = new Vector3[] { pos - right, pos + right };

        foreach (Vector3 origin in rayOrigins)
        {
            RaycastHit hit;
            if (Physics.Raycast(origin, forward, out hit, detectionRange, LayerMask.GetMask("Obstacle")))
            {
                // Steer away from the hit point — reflect off the surface normal
                Vector3 avoidDir = Vector3.Reflect(forward, hit.normal);
                avoidDir.Normalize();
                avoidVel += avoidDir * spn.velocity;
            }
        }

        // Also check proximity to all obstacles for close-range push
        foreach (Transform obs in ObstacleManager.obstacles)
        {
            if (obs == null) continue;
            Vector3 toObs = obs.position - pos;
            float dist = toObs.magnitude;

            if (dist < spn.obstacleDetectDist)
            {
                // Push directly away from obstacle center
                Vector3 pushDir = -toObs.normalized;
                // Stronger push the closer we are
                float strength = 1f - (dist / spn.obstacleDetectDist);
                avoidVel += pushDir * spn.velocity * strength;
            }
        }

        if (avoidVel != Vector3.zero)
            avoidVel.Normalize();

        return avoidVel * spn.velocity;
    }

    void Start() { }
    void Update() { }
}
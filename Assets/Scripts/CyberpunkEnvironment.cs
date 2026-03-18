using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Procedurally builds the cyberpunk neon city environment:
///   - Dark reflective floor with neon grid lines
///   - Solid dark skybox color
///   - Neon city building pillars as scenery (and optional obstacles)
///   - Ambient + fog settings
public class CyberpunkEnvironment : MonoBehaviour
{
    [Header("Floor Settings")]
    public float floorSize = 300f;
    public Material floorMaterial; 

    [Header("Building Pillars")]
    public int buildingCount = 12;
    public float buildingAreaRadius = 120f;
    public bool pillarsAreObstacles = true; 

    [Header("Colors")]
    public Color skyColor = new Color(0.02f, 0.0f, 0.06f); 
    public Color fogColor  = new Color(0.05f, 0.0f, 0.12f);  
    public Color ambientColor = new Color(0.05f, 0.0f, 0.15f); 

    [Header("Grid Lines")]
    public int gridLines = 20;
    public Color gridColor = new Color(0f, 1f, 1f, 0.6f);    

    void Start()
    {
        BuildFloor();
        BuildSky();
        BuildPillars();
        BuildGridLines();
    }

    // FLOOR
    void BuildFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "CyberpunkFloor";
        floor.transform.SetParent(transform);
        floor.transform.position = new Vector3(0f, -60f, 0f);
        floor.transform.localScale = new Vector3(floorSize / 10f, 1f, floorSize / 10f);

        Renderer r = floor.GetComponent<Renderer>();
        if (floorMaterial != null)
        {
            r.material = floorMaterial;
        }
        else
        {
            // Auto-create a dark reflective material
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.05f, 0.05f, 0.08f);
            mat.SetFloat("_Metallic", 0.9f);
            mat.SetFloat("_Glossiness", 0.95f);
            r.material = mat;
        }
    }

    // SKY / ATMOSPHERE
    void BuildSky()
    {
        Camera.main.backgroundColor = skyColor;
        Camera.main.clearFlags = CameraClearFlags.SolidColor;

        RenderSettings.ambientLight = ambientColor;
        RenderSettings.fog = true;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.003f;
    }

    // BUILDING PILLARS
    void BuildPillars()
    {
        Color[] neonAccents = new Color[]
        {
            new Color(1f, 0f, 0.8f), 
            new Color(0f, 1f, 1f),   
            new Color(0.5f, 0f, 1f), 
            new Color(0f, 0.8f, 1f),  
            new Color(1f, 0.9f, 0f),  
        };

        for (int i = 0; i < buildingCount; i++)
        {
            // Scatter buildings in a ring so the center stays open for boids
            float angle = (360f / buildingCount) * i + Random.Range(-15f, 15f);
            float dist  = Random.Range(buildingAreaRadius * 0.5f, buildingAreaRadius);
            float rad   = angle * Mathf.Deg2Rad;

            Vector3 bPos = new Vector3(Mathf.Cos(rad) * dist, 0f, Mathf.Sin(rad) * dist);

            // Random tall pillar
            float height = Random.Range(30f, 120f);
            float width  = Random.Range(8f, 20f);

            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = "Building_" + i;
            pillar.transform.SetParent(transform);
            pillar.transform.position = bPos + new Vector3(0f, (height / 2f) - 60f, 0f);
            pillar.transform.localScale = new Vector3(width, height, width);

            // Dark body
            Renderer r = pillar.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.04f, 0.04f, 0.06f);
            mat.SetFloat("_Metallic", 0.6f);
            mat.SetFloat("_Glossiness", 0.7f);
            r.material = mat;

            // Neon accent strip on top
            Color accent = neonAccents[i % neonAccents.Length];
            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = "NeonStrip_" + i;
            strip.transform.SetParent(pillar.transform);
            strip.transform.localPosition = new Vector3(0f, 0.5f + (2f / height), 0f);
            strip.transform.localScale = new Vector3(1.05f, 2f / height, 1.05f);

            Renderer sr = strip.GetComponent<Renderer>();
            Material sMat = new Material(Shader.Find("Standard"));
            sMat.color = accent;
            sMat.SetColor("_EmissionColor", accent * 3f);
            sMat.EnableKeyword("_EMISSION");
            sr.material = sMat;

            // Optionally register as obstacle
            if (pillarsAreObstacles)
            {
                pillar.AddComponent<ObstacleManager>();
                ObstacleManager om = pillar.GetComponent<ObstacleManager>();
                om.neonColor = accent;
            }
        }
    }

    // NEON GRID LINES ON FLOOR (LineRenderer-based)
    void BuildGridLines()
    {
        GameObject gridParent = new GameObject("NeonGrid");
        gridParent.transform.SetParent(transform);

        float half = floorSize / 2f;
        float step = floorSize / gridLines;
        float yPos = -59.9f; // Just above the floor

        Shader lineShader = Shader.Find("Particles/Standard Unlit");
        if (lineShader == null) lineShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (lineShader == null) lineShader = Shader.Find("Standard");
        Material lineMat = new Material(lineShader);
        lineMat.color = gridColor;

        for (int i = 0; i <= gridLines; i++)
        {
            float t = -half + step * i;

            // Lines along Z axis
            CreateGridLine(gridParent.transform, lineMat,
                new Vector3(t, yPos, -half),
                new Vector3(t, yPos,  half));

            // Lines along X axis
            CreateGridLine(gridParent.transform, lineMat,
                new Vector3(-half, yPos, t),
                new Vector3( half, yPos, t));
        }
    }

    void CreateGridLine(Transform parent, Material mat, Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.SetParent(parent);
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = mat;
        lr.startColor = gridColor;
        lr.endColor   = gridColor;
        lr.startWidth = 0.15f;
        lr.endWidth   = 0.15f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.useWorldSpace = true;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// Cyberpunk glitch effect for both Title and Main scene
public class GlitchEffect : MonoBehaviour
{
    [Header("Glitch Settings")]
    [Range(0f, 1f)]
    public float glitchIntensity = 0.5f;
    public float glitchFrequency = 0.08f;

    [Header("Effect Toggles")]
    public bool enableScanlines = true;
    public bool enableChromatic = true;
    public bool enableJitter = true;
    public bool enableFlicker = true;

    [Header("Scanline Settings")]
    [Range(0f, 1f)]
    public float scanlineOpacity = 0.25f;
    public float scanlineSpeed = 0.5f;

    // Internal components
    private RawImage scanlineOverlay;
    private RawImage chromaticOverlay;
    private RectTransform canvasRect;
    private Canvas parentCanvas;

    // Glitch state
    private float nextGlitchTime;
    private float glitchEndTime;
    private bool isGlitching = false;
    private Vector2 jitterOffset = Vector2.zero;

    // Textures
    private Texture2D scanlineTex;
    private Texture2D chromaticTex;

    void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("[GlitchEffect] Must be inside a Canvas!");
            return;
        }

        canvasRect = parentCanvas.GetComponent<RectTransform>();

        BuildScanlineOverlay();
        BuildChromaticOverlay();

        ScheduleNextGlitch();
    }

    // SCANLINES
    void BuildScanlineOverlay()
    {
        if (!enableScanlines) return;

        // Build a small repeating scanline texture
        int texHeight = 4;
        scanlineTex = new Texture2D(1, texHeight, TextureFormat.RGBA32, false);
        scanlineTex.filterMode = FilterMode.Point;
        scanlineTex.wrapMode = TextureWrapMode.Repeat;

        for (int y = 0; y < texHeight; y++)
        {
            // Every other pair of rows is a dark scanline
            float alpha = (y % 2 == 0) ? scanlineOpacity : 0f;
            scanlineTex.SetPixel(0, y, new Color(0f, 0f, 0f, alpha));
        }
        scanlineTex.Apply();

        // Create UI RawImage stretched over the full screen
        GameObject go = new GameObject("Scanlines");
        go.transform.SetParent(transform, false);
        scanlineOverlay = go.AddComponent<RawImage>();
        scanlineOverlay.texture = scanlineTex;
        scanlineOverlay.color = Color.white;
        scanlineOverlay.raycastTarget = false;

        // UV rect controls scrolling
        scanlineOverlay.uvRect = new Rect(0, 0, 1, 30);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // CHROMATIC ABERRATION OVERLAY
    void BuildChromaticOverlay()
    {
        if (!enableChromatic) return;

        // Simple 1x1 colored texture — we shift it in RGB channels via code
        chromaticTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        chromaticTex.SetPixel(0, 0, Color.white);
        chromaticTex.Apply();

        GameObject go = new GameObject("ChromaticAberration");
        go.transform.SetParent(transform, false);
        chromaticOverlay = go.AddComponent<RawImage>();
        chromaticOverlay.texture = chromaticTex;
        chromaticOverlay.color = new Color(1f, 0f, 1f, 0f);
        chromaticOverlay.raycastTarget = false;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // UPDATE
    void Update()
    {
        ScrollScanlines();
        HandleGlitchTiming();

        if (isGlitching)
        {
            ApplyGlitchFrame();
        }
        else
        {
            ResetGlitchFrame();
        }
    }

    void ScrollScanlines()
    {
        if (scanlineOverlay == null) return;
        Rect uvRect = scanlineOverlay.uvRect;
        uvRect.y += scanlineSpeed * Time.deltaTime;
        scanlineOverlay.uvRect = uvRect;
    }

    void HandleGlitchTiming()
    {
        if (!isGlitching && Time.time >= nextGlitchTime)
        {
            // Start a glitch burst
            isGlitching = true;
            float duration = Random.Range(0.05f, 0.25f) * glitchIntensity;
            glitchEndTime = Time.time + duration;
        }

        if (isGlitching && Time.time >= glitchEndTime)
        {
            isGlitching = false;
            ScheduleNextGlitch();
        }
    }

    void ScheduleNextGlitch()
    {
        // Random delay between glitches — more frequent with higher intensity
        float delay = Random.Range(glitchFrequency * 0.5f, glitchFrequency * 3f);
        delay /= Mathf.Max(glitchIntensity, 0.01f);
        nextGlitchTime = Time.time + delay;
    }

    void ApplyGlitchFrame()
    {
        // --- Chromatic aberration flash ---
        if (chromaticOverlay != null && enableChromatic)
        {
            float alpha = Random.Range(0.05f, 0.2f) * glitchIntensity;
            chromaticOverlay.color = new Color(
                Random.Range(0.8f, 1f),
                0f,
                Random.Range(0.8f, 1f),
                alpha
            );

            // Shift the overlay position slightly for RGB split feel
            RectTransform rt = chromaticOverlay.GetComponent<RectTransform>();
            float shift = Random.Range(-8f, 8f) * glitchIntensity;
            rt.anchoredPosition = new Vector2(shift, 0f);
        }

        // --- Screen jitter ---
        if (enableJitter)
        {
            float jx = Random.Range(-6f, 6f) * glitchIntensity;
            float jy = Random.Range(-3f, 3f) * glitchIntensity;
            jitterOffset = new Vector2(jx, jy);

            RectTransform selfRt = GetComponent<RectTransform>();
            if (selfRt != null)
                selfRt.anchoredPosition = jitterOffset;
        }

        // --- Scanline flicker ---
        if (scanlineOverlay != null && enableFlicker)
        {
            float flicker = Random.Range(0.1f, 0.4f) * glitchIntensity;
            scanlineOverlay.color = new Color(1f, 1f, 1f,
                Random.value > 0.5f ? 1f + flicker : 1f - flicker);
        }
    }

    void ResetGlitchFrame()
    {
        if (chromaticOverlay != null)
        {
            chromaticOverlay.color = new Color(1f, 0f, 1f, 0f);
            RectTransform rt = chromaticOverlay.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
        }

        RectTransform selfRt = GetComponent<RectTransform>();
        if (selfRt != null)
            selfRt.anchoredPosition = Vector2.zero;

        if (scanlineOverlay != null)
            scanlineOverlay.color = Color.white;
    }

    void OnDestroy()
    {
        if (scanlineTex != null) Destroy(scanlineTex);
        if (chromaticTex != null) Destroy(chromaticTex);
    }
}
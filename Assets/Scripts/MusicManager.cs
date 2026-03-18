using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Attach to an empty GameObject in the scene.
/// Assign your electronic/synth AudioClip in the Inspector.
/// Handles smooth fade-in on scene load and persists across scenes.
public class MusicManager : MonoBehaviour
{
    static private MusicManager instance;

    [Header("Music Settings")]
    public AudioClip musicClip;   
    public float volume = 0.6f;
    public float fadeInDuration = 2.0f;  

    private AudioSource audioSource;

    void Awake()
    {
        // Singleton — persist across scene loads (Title → Game)
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.volume = 0f;     
        audioSource.spatialBlend = 0f;   
        audioSource.playOnAwake = false;

        if (musicClip != null)
        {
            audioSource.Play();
            StartCoroutine(FadeIn());
        }
        else
        {
            Debug.LogWarning("[MusicManager] No AudioClip assigned! " +
                "Add a music track to the MusicManager in the Inspector.");
        }
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, elapsed / fadeInDuration);
            yield return null;
        }
        audioSource.volume = volume;
    }

    /// Call this to cross-fade to a different track (e.g., on title screen vs gameplay).
    public void SwapTrack(AudioClip newClip, float fadeDuration = 1.5f)
    {
        StartCoroutine(CrossFade(newClip, fadeDuration));
    }

    IEnumerator CrossFade(AudioClip newClip, float duration)
    {
        float startVol = audioSource.volume;
        float elapsed = 0f;

        // Fade out
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / (duration / 2f));
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();
        elapsed = 0f;

        // Fade in new track
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, elapsed / (duration / 2f));
            yield return null;
        }
        audioSource.volume = volume;
    }
}
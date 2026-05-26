using UnityEngine;

// Gestor central de sonido. Persiste entre escenas (DontDestroyOnLoad).
// Otros scripts llaman SoundManager.Instance.PlayXxx() para reproducir sonidos.
//
// Setup:
//  - Pon este script en un GameObject "SoundManager" en MenuScene
//    (la primera escena que se carga).
//  - Añade DOS AudioSource al mismo GameObject:
//      - sfxSource: Play On Awake = false, Loop = false
//      - musicSource: Play On Awake = false, Loop = true
//  - Arrastra los clips en los campos correspondientes.
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("AudioSources")]
    [Tooltip("Para efectos de sonido (PlayOneShot).")]
    public AudioSource sfxSource;

    [Tooltip("Para música ambiental (loop).")]
    public AudioSource musicSource;

    [Header("Clips - Efectos")]
    [Tooltip("Sonido al abrir una caja (MysteryBox).")]
    public AudioClip boxOpen;

    [Tooltip("Sonido al pulsar un botón de color del puzzle de memoria.")]
    public AudioClip memoryButton;

    [Tooltip("Sonido para botones de menú (hover) y teclado de la consola de código (press).")]
    public AudioClip menuButton;

    [Tooltip("Sonido al girar una válvula (también usado para palancas).")]
    public AudioClip valveTurn;

    [Tooltip("Sonido cuando un rompecabezas se completa (chime).")]
    public AudioClip puzzleSolvedChime;

    [Tooltip("Sonido de error (ej: pulsación incorrecta en memoria).")]
    public AudioClip error;

    [Header("Clips - Música")]
    [Tooltip("Música del menú principal.")]
    public AudioClip menuMusic;

    [Tooltip("Música ambiental del juego (lobby + GameMap).")]
    public AudioClip gameMusic;

    [Header("Volúmenes globales")]
    [Range(0f, 1f)] public float sfxVolume   = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.6f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource != null) musicSource.volume = musicVolume;
        if (sfxSource   != null) sfxSource.volume   = sfxVolume;
    }

    // ── EFECTOS ─────────────────────────────────────────────────────
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayBoxOpen()       => PlaySFX(boxOpen);
    public void PlayMemoryButton()  => PlaySFX(memoryButton);
    public void PlayMenuButton()    => PlaySFX(menuButton);
    public void PlayValveTurn()     => PlaySFX(valveTurn);
    public void PlayPuzzleSolved()  => PlaySFX(puzzleSolvedChime);
    public void PlayError()         => PlaySFX(error);

    // ── MÚSICA ──────────────────────────────────────────────────────
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null) return;
        if (clip == null) { musicSource.Stop(); return; }
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip   = clip;
        musicSource.loop   = loop;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void PlayMenuMusic() => PlayMusic(menuMusic);
    public void PlayGameMusic() => PlayMusic(gameMusic);
    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        if (musicSource != null) musicSource.volume = musicVolume;
    }
}

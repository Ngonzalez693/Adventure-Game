using UnityEngine;

// Pon este script en cada escena para decirle al SoundManager qué música
// reproducir. Solo se activa una vez al cargar la escena.
public class SceneMusic : MonoBehaviour
{
    public enum Track { Menu, Game, None }

    [Tooltip("Música a reproducir cuando entra esta escena.")]
    public Track track = Track.Game;

    private void Start()
    {
        if (SoundManager.Instance == null) return;

        switch (track)
        {
            case Track.Menu: SoundManager.Instance.PlayMenuMusic(); break;
            case Track.Game: SoundManager.Instance.PlayGameMusic(); break;
            case Track.None: SoundManager.Instance.StopMusic();     break;
        }
    }
}

using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
    public static SoundLibrary Instance { get; private set; }

    [System.Serializable]
    public class ZombieSounds
    {
        public AudioClip[] screams;
        public AudioClip[] attacks;
        public AudioClip[] hurts;
        public AudioClip[] dies;
        public AudioClip[] idles;
        public AudioClip[] eating;
    }

    [Header("Character Sounds")]
    public ZombieSounds zombieSounds;

    // Andra ljudkategorier
    [Header("Environment Sounds")]
    public AudioClip[] ambientSounds;
    public AudioClip[] weatherSounds;

    [Header("Player Sounds")]
    public AudioClip[] playerFootsteps;
    public AudioClip[] playerDamage;

    [Header("UI Sounds")]
    public AudioClip buttonClick;
    public AudioClip menuOpen;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Funktion för att hämta ett slumpmässigt ljud från en array
    public AudioClip GetRandomSound(AudioClip[] sounds)
    {
        if (sounds.Length == 0) return null;
        return sounds[Random.Range(0, sounds.Length)];
    }

    // Funktion för att hämta zombieskrik
    public AudioClip GetZombieScream()
    {
        return GetRandomSound(zombieSounds.screams);
    }

    // Liknande metoder för andra ljudtyper
    public AudioClip GetZombieAttack() => GetRandomSound(zombieSounds.attacks);
    public AudioClip GetZombieHurt() => GetRandomSound(zombieSounds.hurts);
    public AudioClip GetZombieDie() => GetRandomSound(zombieSounds.dies);
    public AudioClip GetZombieIdle() => GetRandomSound(zombieSounds.idles);
    public AudioClip GetZombieEating() => GetRandomSound(zombieSounds.eating);
}
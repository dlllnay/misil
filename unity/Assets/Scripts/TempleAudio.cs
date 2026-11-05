using System;
using UnityEngine;
using MuhurluKapi.Interaction;

public enum TempleSound
{
    Arrow, Crystal, Diamond, NyraSeal, OrrinSeal, FirstGate, OrrinGate, Victory, Footstep, PuzzleError
}

public sealed class TempleAudio : MonoBehaviour
{
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField, Range(0f, 1f)] private float musicVolume = .20f;
    [SerializeField, Range(0f, 1f)] private float effectsVolume = .65f;

    private const string ResourcePath = "TempleAudio/";
    private static TempleAudio current;
    private readonly AudioSource[] voices = new AudioSource[8];
    private readonly double[] voiceStarted = new double[8];
    private AudioClip[] effects;
    private AudioSource music;
    private AudioSource victory;
    private double lastArrow = -10;
    private bool won;

    public float MusicVolume
    {
        get => musicVolume;
        set => musicVolume = Mathf.Clamp01(value);
    }

    public float EffectsVolume
    {
        get => effectsVolume;
        set => effectsVolume = Mathf.Clamp01(value);
    }

    private void Awake()
    {
        if (current != null && current != this)
        {
            enabled = false;
            return;
        }
        current = this;
        music = CreateSource("Tapınak müziği");
        music.loop = true;
        music.clip = Resources.Load<AudioClip>(ResourcePath + "TempleAmbient");
        music.volume = 0f;
        victory = CreateSource("Zafer");

        var names = Enum.GetNames(typeof(TempleSound));
        effects = new AudioClip[names.Length];
        for (int i = 0; i < names.Length; i++)
            effects[i] = Resources.Load<AudioClip>(ResourcePath + names[i]);
        for (int i = 0; i < voices.Length; i++)
            voices[i] = CreateSource($"Efekt {i + 1}");
    }

    private void Start()
    {
        if (music != null && music.clip != null)
            music.Play();
    }

    private void Update()
    {
        bool dialogue = playerInteraction != null && playerInteraction.IsDialogueOpen;
        float level = won ? 0f : musicVolume * (dialogue || TempleMenu.IsPaused ? .45f : 1f);
        music.volume = Mathf.MoveTowards(music.volume, level, .28f * Time.unscaledDeltaTime);
        if (won && music.volume == 0f && music.isPlaying)
            music.Stop();
    }

    public static void Play(TempleSound cue, Vector3 position)
    {
        if (current != null && current.isActiveAndEnabled)
            current.PlayCue(cue, position);
    }

    private void PlayCue(TempleSound cue, Vector3 position)
    {
        if (won) return;
        var clip = effects[(int)cue];
        if (cue == TempleSound.Victory)
        {
            won = true;
            if (clip != null && effectsVolume > 0f)
            {
                victory.clip = clip;
                victory.volume = effectsVolume * .86f;
                victory.Play();
            }
            return;
        }
        if (clip == null || effectsVolume <= 0f) return;

        double now = AudioSettings.dspTime;
        if (cue == TempleSound.Arrow)
        {
            if (now - lastArrow < .12) return;
            lastArrow = now;
        }

        int index = 0;
        for (int i = 0; i < voices.Length; i++)
        {
            if (!voices[i].isPlaying)
            {
                index = i;
                break;
            }
            if (voiceStarted[i] < voiceStarted[index])
                index = i;
        }
        var voice = voices[index];
        voice.Stop();
        voice.transform.position = position;
        bool local = cue == TempleSound.Arrow || cue == TempleSound.Diamond || cue == TempleSound.Footstep;
        voice.spatialBlend = local ? 0f : 1f;
        voice.minDistance = 4f;
        voice.maxDistance = 30f;
        voice.volume = effectsVolume * (cue == TempleSound.Footstep ? .36f :
            cue == TempleSound.Arrow ? .56f : .83f);
        voice.pitch = cue == TempleSound.Arrow || cue == TempleSound.Footstep
            ? UnityEngine.Random.Range(.94f, 1.06f) : 1f;
        voice.clip = clip;
        voiceStarted[index] = now;
        voice.Play();
    }

    private AudioSource CreateSource(string label)
    {
        var node = new GameObject(label);
        node.transform.SetParent(transform, false);
        var source = node.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.dopplerLevel = 0;
        source.rolloffMode = AudioRolloffMode.Linear;
        return source;
    }

    private void OnDestroy()
    {
        if (current == this)
            current = null;
    }
}

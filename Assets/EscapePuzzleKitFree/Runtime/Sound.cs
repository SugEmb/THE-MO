using UnityEngine;

namespace HeyHeyThere.EscapePuzzleKitFree
{
    /// <summary>A sound from where something is: its own AudioSource if it has one (your mixer, your falloff), else one made for it.</summary>
    public static class Sound
    {
        public static void Play(Component at, AudioClip clip, float volume = 1f)
        {
            if (clip == null || at == null || !at.gameObject.activeInHierarchy)
                return;
            var source = at.GetComponent<AudioSource>();
            if (source == null)
            {
                source = at.gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 1f;
                source.minDistance = 1.5f;
                source.maxDistance = 20f;
            }
            source.PlayOneShot(clip, volume);
        }
    }
}

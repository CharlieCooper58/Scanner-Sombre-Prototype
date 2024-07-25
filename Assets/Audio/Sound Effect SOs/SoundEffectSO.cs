using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundEffect", menuName = "Sound Effects")]
public class SoundEffectSO : ScriptableObject
{
    [SerializeField] AudioClip[] clips;

    public AudioClip GetSound()
    {
        return clips[Random.Range(0, clips.Length)];
    }
}

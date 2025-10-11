using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static event Action<SoundEvent> OnSoundEmitted;

    public static void EmitSound(SoundEvent sound)
    {
        OnSoundEmitted?.Invoke(sound);
    }
}
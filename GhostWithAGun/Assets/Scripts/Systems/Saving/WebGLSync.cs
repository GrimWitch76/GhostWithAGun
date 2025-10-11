using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLSync
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void SyncFS();
#else
    private static void SyncFS() { }
#endif
    public static void Flush() => SyncFS();
}
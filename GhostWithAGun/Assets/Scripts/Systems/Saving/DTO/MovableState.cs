using System;
using UnityEngine;

[Serializable]
public class MovableState
{
    public float[] pos;   // x,y,z
    public float[] rot;   // x,y,z,w
    public bool destroyed;
    public float health;
}
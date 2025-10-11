using System;
using System.Collections.Generic;

[Serializable]
public class SaveEnvelope
{
    public int night; 
    public int damageValue;
    public List<MovableEntry> movables = new();
    public List<DoorEntry> doors = new();
    public List<LightEntry> lights = new();
}
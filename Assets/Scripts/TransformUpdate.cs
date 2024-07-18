using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformUpdate
{
    public int Tick;
    public Vector3 Position;

    public TransformUpdate(int tick, Vector3 position)
    {
        Tick = tick;
        Position = position;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkTools;

public interface IReconstructable
{
    GameObject gameObject { get; }

    public Vector3 GetPositionAtTick(int tick);
}

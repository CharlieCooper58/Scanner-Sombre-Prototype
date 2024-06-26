using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class Interpolator : MonoBehaviour
{
    [SerializeField] private float timeElapsed;
    [SerializeField] private float timeToReachTarget = 0.05f;
    [SerializeField] private float movementThreshold = 0.05f;

    private readonly List<TransformUpdate> futureTransformUpdates = new List<TransformUpdate>();
    private float squareMovementThreshold;
    private TransformUpdate to;
    private TransformUpdate from;
    private TransformUpdate previous;

    private void Start()
    {
        squareMovementThreshold = movementThreshold * movementThreshold;
        to = new TransformUpdate(ServerWorldManager.instance.currentServerTimerTick.Value, transform.position);
        from = new TransformUpdate(ServerWorldManager.instance.currentServerTimerTick.Value, transform.position);
        previous = new TransformUpdate(ServerWorldManager.instance.currentServerTimerTick.Value, transform.position);


    }

    private void Update()
    {
        for (int i = 0; i < futureTransformUpdates.Count; i++)
        {
            if(ServerWorldManager.instance.currentServerTimerTick.Value >= futureTransformUpdates[i].Tick)
            {
                previous = to;
                to = futureTransformUpdates[i];
                from = new TransformUpdate(ServerWorldManager.instance.currentServerTimerTick.Value - 1, transform.position);
            }
        }
    }
}

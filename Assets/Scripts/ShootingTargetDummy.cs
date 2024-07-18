using NetworkTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShootingTargetDummy : NetworkBehaviour, IReconstructable
{

    CircularBuffer<Vector3> positions;
    [SerializeField] PhysicsSimulationShadow shadowPrefab;
    PhysicsSimulationShadow shadow;

    Vector3 moveDir;
    float moveFlipTimer;
    Rigidbody rb;

    private void Start()
    {
        ServerWorldManager.instance.OnClockTick += Instance_OnClockTick;
        positions = new CircularBuffer<Vector3>(1024);

        moveDir = Vector3.right;
        rb = GetComponent<Rigidbody>();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            shadow = Instantiate(shadowPrefab);
            shadow.SetParent(this);
        }
    }

    private void Instance_OnClockTick(object sender, System.EventArgs e)
    {
        positions.Add(transform.position, ServerWorldManager.instance.currentServerTimerTick.Value % 1024);
    }

    public Vector3 GetPositionAtTick(int tick)
    {
        return positions.Get(tick);
    }

    private void FixedUpdate()
    {
        moveFlipTimer -= Time.deltaTime;
        if(moveFlipTimer <= 0)
        {
            moveDir *= -1;
            rb.velocity = moveDir;
            moveFlipTimer = 3.0f;
        }
    }

}

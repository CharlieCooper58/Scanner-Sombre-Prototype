using NetworkTools;
using PlayerController;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System;

public class ServerWorldManager : NetworkBehaviour
{
    public static ServerWorldManager instance;
    public NetworkTimer timer;

    // The last official tick by the server, set only by the server
    public NetworkVariable<int> currentServerTimerTick;

    // The current timer tick processed by the client's world
    int lastProcessedTick;

    // A list of active players.  Players add themselves to this list when they join the game
    public List<FirstPersonController> activePlayers;
    // If this script runs client-side, also keep track of the local player
    public FirstPersonController localPlayer;

    public event EventHandler OnClockTick;
    const float k_serverTickRate = 60f; // 60 FPS
    private void Awake()
    {
        instance = this;
        timer = new NetworkTimer(k_serverTickRate);
    }

    private void Update()
    {
        timer.Update(Time.deltaTime);
    }
    private void FixedUpdate()
{
        while (timer.ShouldTick())
        {
            if(IsServer) currentServerTimerTick.Value = timer.currentTick;
            if(OnClockTick != null)
                OnClockTick.Invoke(this, EventArgs.Empty);
        }
    }

}

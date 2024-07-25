using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Multiplay;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    // Start is called before the first frame update
    private async void Start()
    {
#if DEDICATED_SERVER
        await MultiplayService.Instance.ReadyServerForPlayersAsync();
        Camera.main.enabled = false;
#endif

    }
}

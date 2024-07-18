using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerManager : NetworkBehaviour
{
    public static MultiplayerManager instance;
    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();

    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();

    }
}

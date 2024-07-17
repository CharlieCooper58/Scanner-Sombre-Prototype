using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class NetworkCanvas : MonoBehaviour
{
    private void Start()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            gameObject.SetActive(false);
        }
    }
    public void StartServer()
    {
        MultiplayerManager.instance.StartServer();
        gameObject.SetActive(false);
    }
    public void StartHost()
    {
        MultiplayerManager.instance.StartHost();
        gameObject.SetActive(false);

    }
    public void StartClient()
    {
        MultiplayerManager.instance.StartClient();
        gameObject.SetActive(false);

    }
}

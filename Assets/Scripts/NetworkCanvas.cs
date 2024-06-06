using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class NetworkCanvas : MonoBehaviour
{
    NetworkManager _networkManager;

    private void Awake()
    {
        _networkManager = GetComponentInParent<NetworkManager>();
    }

    public void StartServer()
    {
        _networkManager.StartServer();
        gameObject.SetActive(false);
    }
    public void StartHost()
    {
        _networkManager.StartHost();
        gameObject.SetActive(false);

    }
    public void StartClient()
    {
        _networkManager.StartClient();
        gameObject.SetActive(false);

    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Multiplay;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance {  get; private set; }

    private Lobby joinedLobby;
    bool alreadyAutoAllocated;
    float heartBeatTimer;

    private IServerQueryHandler serverQueryHandler;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;

        InitializeUnityAuthentication();
    }
    
    private void Update()
    {
        //HandleLobbyHeartbeat();

#if DEDICATED_SERVER
    if (serverQueryHandler != null)
    {
        serverQueryHandler.UpdateServerCheck();
    }
    else
    {
        Debug.LogWarning("serverQueryHandler is null.");
    }
#endif
    }
    private void HandleLobbyHeartbeat() 
    {
        if (IsLobbyHost())
        {
            heartBeatTimer -= Time.deltaTime;
            if(heartBeatTimer <= 0)
            {
                heartBeatTimer = 15f;
                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }
    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }


    private async void InitializeUnityAuthentication()
    {
        if(UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(Random.Range(0, 100000).ToString());
            await UnityServices.InitializeAsync(initializationOptions);
            // Update in the future with better authentication options
#if !DEDICATED_SERVER
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
#endif
#if DEDICATED_SERVER
            Debug.Log("DEDICATED_SERVER LOBBY");
            MultiplayEventCallbacks multiplayEventCallbacks = new MultiplayEventCallbacks();
            multiplayEventCallbacks.Allocate += MultiplayEventCallbacks_Allocate;
            multiplayEventCallbacks.Deallocate += MultiplayEventCallbacks_Deallocate;
            multiplayEventCallbacks.Error += MultiplayEventCallbacks_Error;
            multiplayEventCallbacks.SubscriptionStateChanged += MultiplayEventCallbacks_SubscriptionStateChanged;
            IServerEvents serverEvents = await MultiplayService.Instance.SubscribeToServerEventsAsync(multiplayEventCallbacks);

            serverQueryHandler = await MultiplayService.Instance.StartServerQueryHandlerAsync(4, "MyServerName", "Scan Hunt", "89643", "");

            var serverConfig = MultiplayService.Instance.ServerConfig;
            if(serverConfig.AllocationId != "")
            {
                // Already Allocated
                MultiplayEventCallbacks_Allocate(new MultiplayAllocation("", serverConfig.ServerId, serverConfig.AllocationId));
            }

#endif     
        }

    }

#if DEDICATED_SERVER
    private void MultiplayEventCallbacks_SubscriptionStateChanged(MultiplayServerSubscriptionState obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_SubscriptionStateChanged");
        Debug.Log(obj);
    }

    private void MultiplayEventCallbacks_Error(MultiplayError obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Error");
        Debug.Log(obj.Reason);
    }

    private void MultiplayEventCallbacks_Deallocate(MultiplayDeallocation obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Deallocate");
    }

    private void MultiplayEventCallbacks_Allocate(MultiplayAllocation allocation)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Allocate");
        if (alreadyAutoAllocated)
        {
            Debug.Log("Already auto allocated!");
            return;
        }
        alreadyAutoAllocated = true;

        var serverConfig = MultiplayService.Instance.ServerConfig;
        Debug.Log($"Server ID[{serverConfig.ServerId}]");
        Debug.Log($"AllocationID[{serverConfig.ServerId}]");
        Debug.Log($"Port[{serverConfig.Port}]");
        Debug.Log($"QueryPort[{serverConfig.QueryPort}]");
        Debug.Log($"LogDirectory[{serverConfig.ServerLogDirectory}]");

        string ipv4Address = "0.0.0.0";
        ushort port = serverConfig.Port;
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipv4Address, port, "0.0.0.0");

        MultiplayerManager.instance.StartServer();
        NetworkManager.Singleton.SceneManager.LoadScene("Test Map", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
#endif

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            // Max players currently set at 4, can be increased easily 
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4, new CreateLobbyOptions { IsPrivate = isPrivate });
            MultiplayerManager.instance.StartServer();
            NetworkManager.Singleton.SceneManager.LoadScene("Playground", UnityEngine.SceneManagement.LoadSceneMode.Single);
            
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void QuickJoin()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            MultiplayerManager.instance.StartClient();
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}

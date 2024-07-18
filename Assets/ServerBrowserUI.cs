using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Services.Authentication;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static System.Net.WebRequestMethods;
using UnityEngine.SceneManagement;

public class ServerBrowserUI : NetworkBehaviour
{
    [SerializeField] private Transform serverContainer;
    [SerializeField] private Transform serverTemplate;
    [SerializeField] private Button joinIPButton;
    [SerializeField] private Button createServerButton;
    [SerializeField] private Button refreshServersButton;
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_InputField portInputField;

    public class TokenExchangeResponse
    {
        public string accessToken;
    }


    [Serializable]
    public class TokenExchangeRequest
    {
        public string[] scopes;
    }

    [Serializable]
    public class QueueAllocationRequest
    {
        public string allocationId;
        public int buildConfigurationId;
        public string payload;
        public string regionId;
        public bool restart;
    }


    private enum ServerStatus
    {
        AVAILABLE,
        ONLINE,
        ALLOCATED
    }

    [Serializable]
    public class ListServers
    {
        public Server[] serverList;
    }

    [Serializable]
    public class Server
    {
        public int buildConfigurationID;
        public string buildConfigurationName;
        public string buildName;
        public bool deleted;
        public string fleetID;
        public string fleetName;
        public string hardwareType;
        public int id;
        public string ip;
        public int locationID;
        public string locationName;
        public int machineID;
        public int port;
        public string status;
    }
    [ClientRpc]
    public void SendCurrentSceneClientRPC(string sceneName)
    {
        Debug.Log(sceneName);
    }
    [ServerRpc(RequireOwnership =false)]
    public void GetCurrentSceneServerRPC()
    {
        SendCurrentSceneClientRPC(SceneManager.GetActiveScene().name);
    }
    private void Awake()
    {
        joinIPButton.onClick.AddListener(() =>
        {
            string ipv4Address = ipInputField.text;
            ushort port = ushort.Parse(portInputField.text);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipv4Address, port);

            MultiplayerManager.instance.StartClient();
            GetCurrentSceneServerRPC();

            //SceneManager.LoadScene("Playground");
        });
        refreshServersButton.onClick.AddListener(() =>
        {
            UpdateServerList();
        });

        createServerButton.onClick.AddListener(() =>
        {
            string keyID = "c326ebef-843c-4de4-8062-507f35a3f8dd";
            string keySecret = "HVM29DyhxSI7O1BrFh5SWjGNGKzKJ1Pj";
            byte[] keyByteArray = Encoding.UTF8.GetBytes(keyID + ":" + keySecret);
            string keyBase64 = Convert.ToBase64String(keyByteArray);

            string projectID = "a335c3e8-3196-4c30-80f6-7d9198c8b720";
            string environmentID = "d3544805-4217-4f61-b10c-8d4fcc885c99";
            string tokenRequestURL = $"https://services.api.unity.com/auth/v1/token-exchange?projectId={projectID}&environmentId={environmentID}";
            //string jsonRequestBody = JsonUtility.ToJson(new TokenExchangeRequest
            //{
            //    scopes = new[] { "multiplay.allocations.create", "multiplay.allocations.list" }
            //});
            //WebRequests.PostJson(tokenRequestURL,
            //    (UnityWebRequest unityWebRequest) =>
            //    {
            //        unityWebRequest.SetRequestHeader("Authorization", "Basic " + keyBase64);
            //    },
            //    jsonRequestBody,
            //    (string error) =>
            //    {
            //        Debug.Log("Error: " + error);
            //    },
           //     (string json) =>
            //    {
               //     Debug.Log("Success: " + json);
              //      TokenExchangeResponse tokenExchangeResponse = JsonUtility.FromJson<TokenExchangeResponse>(json);

                    string fleetID = "b6615853-51d9-435c-b031-84f00efd6f2f";
                    string url = $"https://services.api.unity.com/multiplay/allocations/v1/projects/{projectID}/environments/{environmentID}/fleets/{fleetID}/test-allocations";

                    WebRequests.PostJson(url,
                        (UnityWebRequest unityWebRequest) =>
                        {
                            unityWebRequest.SetRequestHeader("Authorization", "Basic " + keyBase64);
                        },
                        JsonUtility.ToJson(new QueueAllocationRequest
                        {
                            // build configuration id and region id once there's a fleet
                            allocationId = "d743d643-7ca5-4692-bc71-7e4b7921d3f8",
                            buildConfigurationId = 1269252,
                            regionId = "2cdc1935-bbf4-4321-8d6d-bf98104f3c61"
                        }),
                        (string error) =>
                        {
                            Debug.Log("Error: " + error);
                        },
                        (string json) =>
                        {
                            Debug.Log("Success: " + json);
                        }
                        );
            //     });
            UpdateServerList();

        });


    }

    private void Start()
    {
        UpdateServerList();
    }

    private void UpdateServerList()
    {
        foreach(Transform t in serverContainer.GetComponentInChildren<Transform>())
        {
            if (!(t.Equals(serverContainer.transform)))
            {
                Destroy(t.gameObject);
            }
        }
        string keyID = "c326ebef-843c-4de4-8062-507f35a3f8dd";
        string keySecret = "HVM29DyhxSI7O1BrFh5SWjGNGKzKJ1Pj";
        byte[] keyByteArray = Encoding.UTF8.GetBytes(keyID + ":" + keySecret);
        string keyBase64 = Convert.ToBase64String(keyByteArray);
        string projectID = "a335c3e8-3196-4c30-80f6-7d9198c8b720";
        string environmentID = "d3544805-4217-4f61-b10c-8d4fcc885c99";
        string getURL = $"https://services.api.unity.com/multiplay/servers/v1/projects/{projectID}/environments/{environmentID}/servers";

        WebRequests.Get(getURL,
            (UnityWebRequest unityWebRequest) => {
                unityWebRequest.SetRequestHeader("Authorization", "Basic " + keyBase64);
            },
            (string error) => { Debug.Log("Error: " + error); },
            (string json) =>
            {
                Debug.Log("Success: " + json);
                ListServers listServers = JsonUtility.FromJson<ListServers>("{\"serverList\":" + json + "}");
                foreach (Server server in listServers.serverList)
                {
                    Debug.Log(server.ip + ":" + server.port);
                    if (server.status == "ONLINE" || server.status == "ALLOCATED")
                    {
                        Transform serverTransform = Instantiate(serverTemplate, serverContainer);
                        serverTransform.gameObject.SetActive(true);
                        serverTransform.GetComponent<ServerBrowserSingleUI>().SetServer(
                            server.ip,
                            (ushort)server.port);
                    }
                }
            });
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Multiplay;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    [SerializeField] Transform[] spawnpoints;

    public static int localPlayerLayer;
    public static int nonLocalPlayerLayer;
    public static int localScanLayer;
    public static int nonLocalScanLayer;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        localPlayerLayer = LayerMask.NameToLayer("Local Player");
        nonLocalPlayerLayer = LayerMask.NameToLayer("Nonlocal Player");
        localScanLayer = LayerMask.NameToLayer("Scan");
        nonLocalScanLayer = LayerMask.NameToLayer("Nonlocal Scan");
    }
    public static void SetGameObjectAndChildrenLayer(GameObject gameObject, int layer)
    {
        if(gameObject == null)
        {
            return;
        }
        gameObject.layer = layer;
        for(int i = 0; i<gameObject.transform.childCount; i++)
        {
            SetGameObjectAndChildrenLayer(gameObject.transform.GetChild(i).gameObject, layer);
        }
    }
    // Start is called before the first frame update
    private async void Start()
    {
#if DEDICATED_SERVER
        await MultiplayService.Instance.ReadyServerForPlayersAsync();
        Camera.main.enabled = false;
#endif

    }

    public Vector3 ReturnRandomSpawnPoint()
    {
        return (spawnpoints[Random.Range(0, spawnpoints.Length)]).position;
    }
}

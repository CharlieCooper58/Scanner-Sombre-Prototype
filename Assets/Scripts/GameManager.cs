using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Multiplay;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    [SerializeField] Transform[] spawnpoints;

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

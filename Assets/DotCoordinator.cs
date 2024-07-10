using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotCoordinator : MonoBehaviour
{
    public static DotCoordinator instance;
    // A smaller grid will look better for operations like erasing, but will perform worse since dots can't be grouped as easily
    [SerializeField] float gridSize;

    Dictionary<Vector3Int, DotManager> localDotManagers;
    DotManager dotManagerPrefab;

    private void Awake()
    {
        instance = this;
    }
    private Vector3Int Vector3ToGridCoordinate(Vector3 position)
    {
        return new Vector3Int(

            Mathf.RoundToInt(position.x / gridSize),
            Mathf.RoundToInt(position.y / gridSize),
            Mathf.RoundToInt(position.z / gridSize)
            );

    }

    public void CreateNewDot(Vector3 position)
    {
        Vector3Int gridCoord = Vector3ToGridCoordinate(position);
        if(!localDotManagers.ContainsKey(gridCoord))
        {
            DotManager newDotmanager = Instantiate(dotManagerPrefab);
            newDotmanager.Initialize(gridCoord);
            localDotManagers[gridCoord] = newDotmanager;
        }
        localDotManagers[gridCoord].SpawnDot(position);
    }
}

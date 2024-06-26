using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerWorldSimulator : MonoBehaviour
{
    public static ServerWorldSimulator instance;
    public List<PhysicsSimulationShadow> simulationGOs = new List<PhysicsSimulationShadow>();

    [SerializeField] string simulationObjectsLayerName;
    int simulationObjectsLayer;
    [SerializeField] string simulationShooterLayerName;
    int simulationShooterLayer;
    [SerializeField] LayerMask simulationRaycastHitMask;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        simulationObjectsLayer = LayerMask.NameToLayer(simulationObjectsLayerName);
        simulationShooterLayer = LayerMask.NameToLayer(simulationShooterLayerName);
    }
    public void CheckRaycastAtTimestamp(PhysicsSimulationShadow shooter, Vector3 position, Vector3 direction, int timeStamp)
    {
        foreach(PhysicsSimulationShadow s in simulationGOs)
        {
            if (s.Equals(shooter))
            {
                s.gameObject.layer = simulationShooterLayer;
                s.SetPositionToMostRecent();
            }
            else
            {
                Debug.Log(s.transform.position);
                s.gameObject.layer = simulationObjectsLayer;
                s.SetPositionBasedOnTick(timeStamp);
                Debug.Log(s.transform.position);
            }
        }
        RaycastHit hit;
        if(Physics.Raycast(position, direction, out hit, 1000f, simulationRaycastHitMask))
        {
            if(hit.collider.gameObject.GetComponent<ShootingTargetDummy>() != null)
            {
                Debug.Log("Hit dummy");
            }
            else
            {
                Debug.Log("Hit!");
            }
            Debug.DrawLine(position, hit.point);
        }
        else
        {
            Debug.Log("Missed, fool");
        }
    }
}

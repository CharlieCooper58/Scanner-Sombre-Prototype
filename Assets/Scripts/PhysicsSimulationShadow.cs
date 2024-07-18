using PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsSimulationShadow : MonoBehaviour
{
    IReconstructable parent;
    CapsuleCollider capsuleCollider;

    private void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        

        ServerWorldSimulator.instance.simulationGOs.Add(this);
    }
    public void SetParent(IReconstructable parent)
    {
        this.parent = parent;
        transform.localScale = parent.gameObject.transform.localScale;
        CharacterController controller = parent.gameObject.GetComponent<CharacterController>();
        if(controller != null)
        {
            capsuleCollider.height = controller.height;
            capsuleCollider.radius = controller.radius;
        }
        else
        {
            CapsuleCollider parentCollider = parent.gameObject.GetComponent<CapsuleCollider>();
            capsuleCollider.height = parentCollider.height;
            capsuleCollider.radius = parentCollider.radius;
        }
        
    }
    public void SetPositionBasedOnTick(int tick)
    {
        transform.position = parent.GetPositionAtTick(tick);
    }
    public void SetPositionToMostRecent()
    {
        transform.position = parent.gameObject.transform.position;
    }
}

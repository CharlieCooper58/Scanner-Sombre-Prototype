using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using Unity.Netcode;
public class DotManager : NetworkBehaviour
{
    public static DotManager instance;
    [SerializeField] Mesh dotMesh;
    [SerializeField] Material dotMaterial;
    List<List<Matrix4x4>> matrixBatches;
    [SerializeField] float dotScale;
    int dotLayer;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        matrixBatches = new List<List<Matrix4x4>>
        {
            new List<Matrix4x4>()
        };
        dotLayer = LayerMask.NameToLayer("Scan");
    }
    public void SpawnDot(Vector3 dotPosition)
    {
        Matrix4x4 newDotMatrix = Matrix4x4.TRS(dotPosition, Quaternion.identity, dotScale*Vector3.one);
        if (matrixBatches.Last().Count > 999)
        {
            matrixBatches.Add(new List<Matrix4x4>());
        }
        matrixBatches.Last().Add(newDotMatrix);

    }

    private void Update()
    {
        if(matrixBatches[0].Count == 0) { return; }
        foreach(List<Matrix4x4> matrixBatch in matrixBatches) 
        {
            Graphics.DrawMeshInstanced(dotMesh, 0, dotMaterial, matrixBatch, null, ShadowCastingMode.Off, false, layer:dotLayer);
        }
    }
}

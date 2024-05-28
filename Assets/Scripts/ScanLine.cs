using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ScanLine : MonoBehaviour
{
    LineRenderer scanRenderer;
    [SerializeField] float lineShowTime = 0.1f;
    Vector3 endPoint;
    ScanLinesPool scanLinesPool;

    bool wasInitialized;
    bool wasSet;

    //Vector3 startPoint = new Vector3(0, 0, 0.1f);
    private void Awake()
    {
        scanRenderer = GetComponent<LineRenderer>();
    }

    public void Initialize(ScanLinesPool pool)
    {
        scanLinesPool = pool;
        wasInitialized = true;
    }
    public void SetLine(Vector3 hitPoint)
    {
        scanRenderer.SetPosition(0, Vector3.zero);
        endPoint = hitPoint;
        wasSet = true;
       // scanRenderer.SetPosition(1, hitPoint);
        StartCoroutine("DestroyThis");
    }
    IEnumerator DestroyThis()
    {
        yield return new WaitForSeconds(lineShowTime);
        scanLinesPool.Return(this);
    }

    private void Update()
    {
        scanRenderer.SetPosition(1, transform.InverseTransformPoint(endPoint));
        if(!wasSet)
        {
            Debug.Log("Wasn't set");
            wasSet = true;
        }
        if (!wasInitialized)
        {
            Debug.Log("Wasn't initialized");
            wasInitialized = true;
        }
    }
}

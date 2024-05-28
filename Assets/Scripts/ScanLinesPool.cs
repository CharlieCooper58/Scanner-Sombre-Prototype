using System.Collections.Generic;
using UnityEngine;

public class ScanLinesPool : MonoBehaviour
{
    public ScanLine prefab;
    private Queue<ScanLine> pool = new Queue<ScanLine>();

    public ScanLine Get()
    {
        if (pool.Count == 0)
        {
            AddObjects(1);
        }
        ScanLine newLine = pool.Dequeue();
        newLine.gameObject.SetActive(true);
        newLine.Initialize(this);
        return newLine;
    }

    public void Return(ScanLine objectToReturn)
    {
        objectToReturn.gameObject.SetActive(false);
        pool.Enqueue(objectToReturn);
    }

    private void AddObjects(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }
}

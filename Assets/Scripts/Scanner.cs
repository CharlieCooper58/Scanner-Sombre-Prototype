using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class Scanner : NetworkBehaviour
{
    bool isScanning;
    float scanTimeElapsed;
    [SerializeField] float timeBetweenScans = .1f;
    [SerializeField] float angleSpread;

    [SerializeField] ScanLine scanLinePrefab;
    [SerializeField] LayerMask scanMask;

    [SerializeField] float spreadChangeSpeed;
    [SerializeField] float minSpread = 5f;
    [SerializeField] float maxSpread = 30f;

    [SerializeField] AudioSource audioSource;

    ScanLinesPool scanLinesPool;

    private void Awake()
    {
        scanLinesPool = GetComponentInChildren<ScanLinesPool>();
    }
    public void SetIsScanning(bool isScanning)
    {
        this.isScanning = isScanning;
    }
    public void ChangeSpread(float spreadDelta)
    {
        angleSpread = Mathf.Clamp(angleSpread+spreadChangeSpeed * spreadDelta, minSpread, maxSpread);
        
    }

    private void Update()
    {
        if (isScanning) 
        {
            ScanTerrain();
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }
    public void ScanTerrain()
    {
        scanTimeElapsed += Time.deltaTime;
        while (scanTimeElapsed >= timeBetweenScans)
        {
            float randomSeed = Random.Range(-angleSpread, angleSpread);
            float randomAngle = Random.Range(0, 2 * Mathf.PI);

           

            // Generate a random direction within angleSpread of transform.forward
            Vector3 randomDirection = Quaternion.AngleAxis(randomSeed*Mathf.Cos(randomAngle), transform.right)*Quaternion.AngleAxis(randomSeed*Mathf.Sin(randomAngle), transform.up) * transform.forward;
            
            // Perform the raycast  
            RaycastHit hit;
            if (Physics.Raycast(transform.position, randomDirection, out hit, scanMask))
            {
                ScanTerrainServerRPC(hit.point);
            }
            else
            {
                ScanLine scanLine = scanLinesPool.Get();
                scanLine.SetLine(transform.position + randomDirection * 100f);
            }
            scanTimeElapsed -= timeBetweenScans;
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void ScanTerrainServerRPC(Vector3 position)
    {
        ScanTerrainClientRPC(position);
    }
    [ClientRpc]
    public void ScanTerrainClientRPC(Vector3 position)
    {
        // Instantiate a scan point prefab at the hit point
        DotManager.instance.SpawnDot(position);
        ScanLine scanLine = scanLinesPool.Get();
        scanLine.SetLine(position);
    }
    /*
    private void Scan()
    {
        // Check if it's time to scan based on the elapsed time
        scanTimeElapsed += Time.deltaTime;
        while(scanTimeElapsed >= timeBetweenScans)
        {
            float randomSeed = Random.Range(0, 360);
            float randomAmplitude = Random.Range(0, angleSpread);
            // Generate a random direction within angleSpread of transform.forward
            Vector3 randomDirection = Quaternion.Euler(
                randomAmplitude*Mathf.Cos(randomSeed),
                randomAmplitude * Mathf.Sin(randomSeed),
                0) * transform.forward;

            ScanTerrainj
            // Perform the raycast
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position, randomDirection, out hit, scanMask))
            {
                // Instantiate a scan point prefab at the hit point
                DotManager.instance.SpawnDot(hit.point);
                scanLine.SetLine(transform.position + transform.forward * 0.2f, hit.point);
            }
            else
            {
                scanLine.SetLine(transform.position+transform.forward*0.2f, transform.position + randomDirection * 100f);
            }

            scanTimeElapsed -= timeBetweenScans;
        }

        
    }*/
}

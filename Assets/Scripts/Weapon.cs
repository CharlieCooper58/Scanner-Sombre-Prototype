using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    public Transform barrelEndpoint;
    NetworkVariable<float> reloadTimer = new NetworkVariable<float>();
    [SerializeField] float reloadTimerMax;

    [SerializeField] AudioSource gunAudio;
    [SerializeField] SoundEffectSO gunshotSounds;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            reloadTimer.Value = 0;
        }
    }
    public void Fire()
    {
        reloadTimer.Value = reloadTimerMax;
        FireSoundClientRPC();
        Debug.Log("Trying to shoot");
    }

    [ClientRpc]
    public void FireSoundClientRPC()
    {
        gunAudio.PlayOneShot(gunshotSounds.GetSound());
        Debug.Log("Please bang");
    }
    public bool CanFire()
    {
        return reloadTimer.Value <= 0;
    }
    private void Update()
    {
        if (IsServer)
        {
            reloadTimer.Value -= Time.deltaTime;
        }
    }
}

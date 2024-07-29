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

    [SerializeField] GunshotParticleController gunshotParticleControllerPrefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            reloadTimer.Value = 0;
        }
    }
    public void FireWeaponInstantFeedback(Vector3 hitPoint)
    {
        gunAudio.PlayOneShot(gunshotSounds.GetSound());
        var gunshotParticleController = Instantiate(gunshotParticleControllerPrefab, barrelEndpoint.position, barrelEndpoint.rotation);
        gunshotParticleController.FireGunParticles(Vector3.Distance(barrelEndpoint.position, hitPoint));
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
        if (IsOwner)
        {
            return;
        }
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

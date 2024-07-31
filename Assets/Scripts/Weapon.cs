using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    public Transform barrelEndpoint;
    NetworkVariable<float> reloadTimer = new NetworkVariable<float>();
    [SerializeField] float reloadTimerMax;

    float reloadTimerLocal;
    float reloadTimerLocalMax;

    [SerializeField] AudioSource gunAudio;
    [SerializeField] SoundEffectSO gunshotSounds;

    [SerializeField] GunshotParticleController gunshotParticleControllerPrefab;

    [SerializeField] float positionRecoilX;
    [SerializeField] float positionRecoilY;
    [SerializeField] float positionRecoilZ;
    [SerializeField] float rotationRecoilX;
    [SerializeField] float rotationRecoilY;
    [SerializeField] float rotationRecoilZ;

    private void Awake()
    {
        reloadTimerLocalMax = reloadTimerMax;
    }
    public struct ShotRecoilResults
    {
        public Vector3 positionRecoil;
        public Vector3 rotationRecoil;

        public ShotRecoilResults(float prX, float prY, float prZ, float rrX, float rrY, float rrZ)
        {
            positionRecoil = new Vector3(prX, prY, prZ);
            rotationRecoil = new Vector3 (rrX, rrY, rrZ);
        }
    }

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
        gunshotParticleController.FireGunParticles(barrelEndpoint.position, hitPoint);
        reloadTimerLocal = reloadTimerLocalMax;
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
        return reloadTimer.Value <= 0 && reloadTimerLocal <= 0 ;
    }
    private void Update()
    {
        if (IsServer)
        {
            reloadTimer.Value -= Time.deltaTime;
        }
        reloadTimerLocal -= Time.deltaTime;
    }

    public ShotRecoilResults GetShotRecoil()
    {
        return new ShotRecoilResults(
            Random.Range(-positionRecoilX, positionRecoilX),
            Random.Range(-positionRecoilY, positionRecoilY),
            positionRecoilZ,
            rotationRecoilX,
            Random.Range(-rotationRecoilY, rotationRecoilY),
            Random.Range(-rotationRecoilZ, rotationRecoilZ)
            );
    }
}

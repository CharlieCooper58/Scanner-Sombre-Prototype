using PlayerController;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerWeaponController : NetworkBehaviour
{
    FirstPersonController playerController;
    PlayerInputHandler playerInputHandler;
    [SerializeField] Weapon equippedWeapon;
    [SerializeField] Transform cameraRotationThingy;
    Vector3 screenCenter;
    [SerializeField] LayerMask canHitWithGunMask;

    [SerializeField] LineRenderer shotTracer;

    private void Awake()
    {
        playerController = GetComponent<FirstPersonController>();
        playerInputHandler = GetComponent<PlayerInputHandler>();
    }
    private void Start()
    {
        canHitWithGunMask = ~LayerMask.GetMask("Local Player");
        screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    }
    private void Update()
    {
        if (playerInputHandler.triggerIsHeld)
        {
            TryShoot();
        }
    }
    public void TryShoot() 
    {
        if (equippedWeapon != null && equippedWeapon.CanFire())
        {
            Debug.Log("Bang");
            Vector3 hitPoint;
            // Create a ray from the center of the screen
            Ray ray = Camera.main.ScreenPointToRay(screenCenter);

            // Variable to store information about the object hit
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit, 100f, canHitWithGunMask))
            {
                // If an object is hit, return its position
                hitPoint = hit.point;
            }
            else
            {
                // If no object is hit, return the player's position plus 100 times the ray direction
                hitPoint = cameraRotationThingy.position + ray.direction * 100f;
            }
            shotTracer.SetPosition(0, equippedWeapon.barrelEndpoint.position);
            shotTracer.SetPosition(1, hitPoint);
            FireWeaponServerRPC(hitPoint - cameraRotationThingy.position, ServerWorldManager.instance.currentServerTimerTick.Value);
            equippedWeapon.FireWeaponInstantFeedback(hit.point);
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void FireWeaponServerRPC(Vector3 aimDirection, int tickValueAtTimeOfShot)
    {
        playerController.HandleServerTick();

        ServerWorldSimulator.instance.CheckRaycastAtTimestamp(playerController.shadow, cameraRotationThingy.position, aimDirection, tickValueAtTimeOfShot);
        equippedWeapon.Fire();
    }

}

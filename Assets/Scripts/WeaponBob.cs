using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// This entire script is taken from a video I found online
// https://www.youtube.com/watch?v=DR4fTllQnXg
// Making changes as necessary
// I deem this functional for now
public class WeaponBob : MonoBehaviour
{
    [Header("Settings")]
    public bool sway = true;
    public bool swayRotation = true;
    public bool bobOffset = true;
    public bool bobRotation = true;

    [SerializeField] float lerpPositionSmoothing = 10f;
    [SerializeField] float lerpRotationSmoothing = 12f;

    [Header("Sway Settings")]
    public float step = 0.01f;
    public float maxStepDistance = 0.06f;
    Vector3 swayPos;

    public float rotationStep = 4f;
    public float maxRotationStep = 5f;
    Vector3 swayEulerRot;

    [Header("Bobbing Settings")]
    // Represents the time elapsed, scaled by our movement
    public float speedCurve;
    float curveSin { get=>Mathf.Sin(speedCurve);}
    float curveCos { get=>Mathf.Cos(speedCurve);}
    public Vector3 bobTravelLimit = Vector3.one * 0.025f; // How much is bobbing affected by the player's movement direction?
    public Vector3 bobWaveLimit = Vector3.one * 0.01f; // How much does the weapon naturally bob while moving?

    Vector3 bobPosition;

    public Vector3 bobRotationMultiplier = Vector3.one * 0.06f;
    Vector3 bobEulerRotation;

    [Header("Recoil Settings")]
    Vector3 recoilTargetOffset;
    [SerializeField] float recoilPositionResponsiveness;
    [SerializeField] float recoilPositionReturnSpeed;
    Vector3 recoilAngularOffset;
    [SerializeField] float recoilRotationResponsiveness;
    [SerializeField] float recoilRotationReturnSpeed;

    [SerializeField] float recoilPositionOffsetMax;
    [SerializeField] float recoilAngleOffsetMax;

    [Header("Inputs")]
    Vector3 lookInput;
    Vector3 movementInput;
    Vector3 velocity;
    bool grounded;
    
    public void SetInputValues(Vector3 lookInput, Vector3 movementInput, Vector3 velocity, bool grounded)
    {
        this.lookInput = lookInput;
        this.movementInput = movementInput;
        this.velocity = velocity;
        this.grounded = grounded;
    }

    private void FixedUpdate()
    {
        UpdateSpeedCurve();
        CalculateBobOffset();
        CalculateBobRotation();
        CalculateSwayPosition();
        CalculateSwayRotation();

        ResetRecoilTowardsZero();

        transform.localPosition = Vector3.Lerp(transform.localPosition, swayPos+bobPosition + recoilTargetOffset, Time.deltaTime * lerpPositionSmoothing);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(swayEulerRot)*Quaternion.Euler(bobEulerRotation) * Quaternion.Euler(recoilAngularOffset), Time.deltaTime * lerpRotationSmoothing);
    }
    #region Sway
    void CalculateSwayPosition()
    {
        if (!sway)
        {
            swayPos = Vector3.zero;
            return;
        }
        Vector2 positionInvertLook = lookInput * -step;
        positionInvertLook.x = Mathf.Clamp(positionInvertLook.x, -maxStepDistance, maxStepDistance);
        positionInvertLook.y = Mathf.Clamp(positionInvertLook.y, -maxStepDistance, maxStepDistance);

        swayPos = positionInvertLook;
    }
    void CalculateSwayRotation()
    {
        if (!swayRotation)
        {
            swayEulerRot = Vector3.zero;
            return;
        }
        Vector2 rotationInvertLook = lookInput * -step;
        rotationInvertLook.x = Mathf.Clamp(rotationInvertLook.x, -maxRotationStep, maxRotationStep);
        rotationInvertLook.y = Mathf.Clamp(rotationInvertLook.y, -maxRotationStep, maxRotationStep);

        swayEulerRot = new Vector3(rotationInvertLook.y, rotationInvertLook.x, rotationInvertLook.x);
    }
    #endregion

    #region Bobbing
    void UpdateSpeedCurve()
    {
        speedCurve += Time.deltaTime * ((grounded ? velocity.magnitude : 1f) + 0.01f);
    }
    void CalculateBobOffset()
    {
        bobPosition.x = (curveCos*bobWaveLimit.x * (grounded?1:0)) - (movementInput.x*bobTravelLimit.x);
        bobPosition.y = (curveSin * bobWaveLimit.y - (velocity.y * bobTravelLimit.y));
        bobPosition.z = -(movementInput.z * bobTravelLimit.z);

    }
    private void CalculateBobRotation()
    {
        bobEulerRotation.x = (movementInput != Vector3.zero ? bobRotationMultiplier.x * Mathf.Sin(2 * speedCurve) : bobRotationMultiplier.x * Mathf.Sin(2 * speedCurve) / 2);
        bobEulerRotation.y = (movementInput != Vector3.zero ? bobRotationMultiplier.y * curveCos: 0);
        bobEulerRotation.z = (movementInput != Vector3.zero ? bobRotationMultiplier.z * curveCos*movementInput.x:0);

    }
    #endregion

    #region Recoil
    private void ResetRecoilTowardsZero()
    {
        recoilTargetOffset = Vector3.Lerp(recoilTargetOffset, Vector3.zero, recoilPositionReturnSpeed * Time.deltaTime);
        recoilAngularOffset = Vector3.Lerp(recoilAngularOffset, Vector3.zero, recoilRotationReturnSpeed * Time.deltaTime);

    }
    public void SetRecoil(Weapon.ShotRecoilResults recoil)
    {
        recoilTargetOffset = Vector3.ClampMagnitude(recoilTargetOffset+recoilPositionResponsiveness * recoil.positionRecoil, recoilPositionOffsetMax);
        recoilAngularOffset = Vector3.ClampMagnitude(recoilAngularOffset+recoilRotationResponsiveness * recoil.rotationRecoil, recoilAngleOffsetMax);
    }
    #endregion
}

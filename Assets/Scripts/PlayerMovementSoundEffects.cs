using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementSoundEffects : MonoBehaviour
{
    [SerializeField] SoundEffectSO footstepSoundSO;
    [SerializeField] SoundEffectSO jumpSoundSO;
    [SerializeField] SoundEffectSO landingSoundSO;
    [SerializeField] AudioSource footSoundsSource;

    [SerializeField] float footstepSoundWalkingFrequency;
    [SerializeField] float footstepSoundSprintingFrequency;
    [SerializeField] float footstepSoundCrouchedFrequency;
    [SerializeField] float footstepSoundWalkingVolume;
    [SerializeField] float footstepSoundSprintingVolume;
    [SerializeField] float footstepSoundCrouchedVolume;

    float footstepSoundTimer;

    public enum MovementStyle
    {
        StoppedOrAirborne,
        Walking,
        Sprinting,
        Crouched
    }
    MovementStyle currentMovement;

    public void SetIsMoving(MovementStyle movementStyle)
    {
        this.currentMovement = movementStyle;
    }

    private void Update()
    {
        if(currentMovement != MovementStyle.StoppedOrAirborne)
        {
            footstepSoundTimer -= Time.deltaTime;
            if(footstepSoundTimer <= 0)
            {
                Debug.Log("Footstep, playing sound");
                float vol = currentMovement == MovementStyle.Walking ? footstepSoundWalkingVolume : (currentMovement == MovementStyle.Sprinting ? footstepSoundSprintingVolume : footstepSoundCrouchedVolume);
                footstepSoundTimer = currentMovement == MovementStyle.Walking ? footstepSoundWalkingFrequency : (currentMovement == MovementStyle.Sprinting ? footstepSoundSprintingFrequency : footstepSoundCrouchedFrequency);

                footSoundsSource.PlayOneShot(footstepSoundSO.GetSound(), vol);
            }
        }
        else
        {
            footstepSoundTimer = 0;
        }
    }

}

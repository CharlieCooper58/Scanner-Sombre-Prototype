using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInputHandler : NetworkBehaviour
{
    public PlayerControls playerControls;
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    Scanner _scanner;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;
    private void Awake()
    {
        _scanner = GetComponentInChildren<Scanner>();
    }
    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            SetCursorState(cursorLocked);

            playerControls = new PlayerControls();
            playerControls.Enable();
            playerControls.Player.Move.performed += x => move = x.ReadValue<Vector2>();
            playerControls.Player.Look.performed += x => look = x.ReadValue<Vector2>();
            playerControls.Player.Jump.performed += x => Jump();
            playerControls.Player.Sprint.performed += x => Sprint();
            playerControls.Player.Scan.performed += x => Scan();
            playerControls.Player.ChangeScanSpread.performed += x => ChangeScanSpread(x.ReadValue<Vector2>());
        }
    }

    private void Jump()
    {
        jump = playerControls.Player.Jump.IsPressed();
    }
    private void Sprint()
    {
        sprint = playerControls.Player.Sprint.IsPressed();
    }
    private void Scan()
    {
        _scanner.SetIsScanning(playerControls.Player.Scan.IsPressed());
    }
    private void ChangeScanSpread(Vector2 scanChangeInput)
    {
        _scanner.ChangeSpread(scanChangeInput.y);
    }
    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinbutton;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            return;
        });
        createLobbyButton.onClick.AddListener(() =>
        {
            LobbyManager.instance.CreateLobby("LobbyName", false);
        });
        quickJoinbutton.onClick.AddListener(() =>
        {
            LobbyManager.instance.QuickJoin();
        });
    }
}

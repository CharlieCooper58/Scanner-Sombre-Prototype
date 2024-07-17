using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuDedicatedServer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if DEDICATED_SERVER
        Debug.Log("DEDICATED_SERVER V 0.1");
        SceneManager.LoadScene("Playground");
#endif
    }
}

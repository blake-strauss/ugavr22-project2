using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    //directly assigned in the inspector
    [SerializeField] VRPlayer player;
    [SerializeField] Transform startLocation;

    // Start is called before the first frame update
    void Start()
    {
        player.doTeleport(startLocation.position, startLocation.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

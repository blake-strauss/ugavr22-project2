using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Dancer : MonoBehaviour
{
    [SerializeField] Animator anim;

    public Transform head;
    public float distanceFromPlayer;
    public AudioSource audioSource;

    private bool alreadyGreeted;
    private bool dancing;
    private float viewRange = 4.0f;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnAnimatorIK(int layerIndex)
    {
        anim.SetLookAtPosition(head.position);
        if (distanceFromPlayer < viewRange && !anim.GetCurrentAnimatorStateInfo(0).IsName("dance")) anim.SetLookAtWeight(1.0f);
        else anim.SetLookAtWeight(0);
    }

    // Update is called once per frame
    void Update()
    {
        distanceFromPlayer = Vector3.Distance(head.transform.position, transform.position);

        if (distanceFromPlayer < viewRange && !alreadyGreeted)
        {
            preformWave();
        }
        else if (distanceFromPlayer > viewRange)
        {
            alreadyGreeted = false;
        }
    }

    public void preformDance() //assigned in inspector UnityEvent
    {
        anim.SetTrigger("dance");
        AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);
    }

    public void preformWave()
    {
        anim.SetTrigger("wave");
        alreadyGreeted = true;
    }
}

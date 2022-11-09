using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Button : MonoBehaviour
{
    private bool isPressed;
    public UnityEvent onPressed, onReleased;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        onPressed.Invoke();
        isPressed = true;
    }
    
    private void OnTriggerExit(Collider other)
    {
        onReleased.Invoke();
        isPressed = false;
    }
}

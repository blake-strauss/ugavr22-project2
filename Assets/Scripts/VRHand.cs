using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRHand : MonoBehaviour
{
    public List<VRGrabbable> grabbables = new List<VRGrabbable>();
    public Transform grabOffset;

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
        //if there is no rigidbody on VRHand, do not add as a grabbable object
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
        {
            return;
        }

        VRGrabbable grabbable = rb.GetComponent<VRGrabbable>();
        if (grabbable == null)
        {
            return;
        }

        grabbables.Add(grabbable);
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null)
        {
            return;
        }

        VRGrabbable grabbable = rb.GetComponent<VRGrabbable>();
        if (grabbable == null)
        {
            return;
        }

        grabbables.Remove(grabbable);
    }
}
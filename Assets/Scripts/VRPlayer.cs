using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRPlayer : MonoBehaviour
{
	public enum GRIP_STATE { OPEN, OBJECT, AIR }
	public enum TELEPORT_STATE { ACTIVE, WAITING }
	public enum SNAP_STATE { ACTIVE, WAITING }

	public float gripThresholdActivate;
	public float gripThresholdDeactivate;
	public float teleportThresholdActivate;
	public float teleportThresholdDeactivate;
	public float snapThresholdActivate;
	public float snapThresholdDeactivate;
	public float snapDegree;

	public float[] gripValues = new float[2] { 0, 0 };
	public Vector2[] joyValues = new Vector2[2];
	
	public GRIP_STATE[] gripStates = new GRIP_STATE[2] { GRIP_STATE.OPEN, GRIP_STATE.OPEN };
	public TELEPORT_STATE[] teleportStates = new TELEPORT_STATE[] { TELEPORT_STATE.WAITING, TELEPORT_STATE.WAITING };
	public SNAP_STATE[] snapStates = new SNAP_STATE[] { SNAP_STATE.WAITING, SNAP_STATE.WAITING };
	
	public Vector3[] gripLocations = new Vector3[2];
	Vector3[] cameraRigGripLocation = new Vector3[2];
	Vector3[] displacements = new Vector3[2];
	
	public VRHand[] hands = new VRHand[2];
	
	public VRGrabbable[] grabbedObjects = new VRGrabbable[2] { null, null };
	
	public GameObject teleporterArcPointPrefab;
	public Transform[] teleporterStartPoses = new Transform[2];
	public Transform[] teleporterTargetPoses = new Transform[2];
	public bool[] teleporterValid = new bool[2];
	public float teleporterStartSpeed;
	public float teleporterMaxDistance;
	
	public Transform head; //the vr camera

	// Start is called before the first frame update
    void Start()
    {
        
    }
    
    Vector3 getFootPositionWorld()
    {
	    Vector3 headInWorld = head.position;
	    Vector3 playCenter = transform.position;
	    Vector3 feetInWorld = headInWorld;
	    feetInWorld.y = playCenter.y;
        
	    return feetInWorld;
    }
    
    public void doTeleport(Vector3 targetFootPosWorld, Quaternion rotation)
    {
	    Vector3 offset = targetFootPosWorld - getFootPositionWorld();
	    transform.position = transform.position + offset;
	    transform.rotation = rotation;
    }

    void teleport(int handIndex) //handle teleporting states
    {
	    if (teleportStates[handIndex] == TELEPORT_STATE.WAITING) //not currently teleporting
	    {
		    if (joyValues[handIndex].y > teleportThresholdActivate)
		    {
			    teleportStates[handIndex] = TELEPORT_STATE.ACTIVE;
		    }
	    }
	    else if (teleportStates[handIndex] == TELEPORT_STATE.ACTIVE) //in the process of teleporting
	    {
		    if (joyValues[handIndex].y < teleportThresholdDeactivate) //when the player releases joystick
		    {
			    if (teleporterValid[handIndex])
                {
                    doTeleport(teleporterTargetPoses[handIndex].position, transform.rotation);
                }
                teleportStates[handIndex] = TELEPORT_STATE.WAITING;
                teleporterTargetPoses[handIndex].gameObject.SetActive(false);
		    }
            else
            {
                //adjust the teleporter visualization
                //shoot a projectile out from the start point, in the direction of the start point forward at a velocity
                Vector3 currentPosition = teleporterStartPoses[handIndex].position;
                Vector3 currentVelocity = teleporterStartPoses[handIndex].forward * teleporterStartSpeed;
                float currentDistance = 0;
                float deltaTime = .02f;
                teleporterValid[handIndex] = false;
                while (currentDistance < teleporterMaxDistance && !teleporterValid[handIndex])
                {
                    Vector3 nextPosition = currentPosition + currentVelocity * deltaTime;
                    Vector3 nextVelocity = currentVelocity + Vector3.up * (-9.81f * deltaTime);

                    Vector3 between = nextPosition - currentPosition;
                    RaycastHit[] hits = Physics.RaycastAll(currentPosition, between.normalized, between.magnitude);

                    teleporterTargetPoses[handIndex].gameObject.SetActive(false); //deactivate every frame
                    foreach (RaycastHit h in hits)
                    {
                        if (h.normal.y > .9f) //partially broken, will go through slanted surfaces
                        {
                            teleporterTargetPoses[handIndex].position = h.point;
                            teleporterTargetPoses[handIndex].up = h.normal;
                            teleporterValid[handIndex] = true;
                            teleporterTargetPoses[handIndex].gameObject.SetActive(true); //deactivate every frame
                            break;
                        }
                    }
                    GameObject point = GameObject.Instantiate(teleporterArcPointPrefab);
                    point.transform.parent = teleporterStartPoses[handIndex];
                    point.transform.position = nextPosition;
                    point.transform.forward = nextVelocity.normalized;
                    currentDistance += between.magnitude;

                    currentPosition = nextPosition;
                    currentVelocity = nextVelocity;
                }
            }
	    }
    }

    void grabbing(int handIndex)
    {
			
			displacements[handIndex] = Vector3.zero; //used for grab locomotion
			//begin grip finite state machine
			if (gripStates[handIndex] == GRIP_STATE.AIR) //gripping the air, so move player
			{

                if(gripValues[handIndex] < gripThresholdDeactivate) //user has let go
				{
                    gripStates[handIndex] = GRIP_STATE.OPEN;
				}
                else 
				{
					//calculate player position based on grip location displacement
                    Vector3 handInTracking = transform.worldToLocalMatrix.MultiplyPoint(hands[handIndex].transform.position);
                    Vector3 between = handInTracking - gripLocations[handIndex];

                    displacements[handIndex] = transform.TransformVector(-between);
				}
			} 
            else if (gripStates[handIndex] == GRIP_STATE.OBJECT) //gripping an object
			{
                if (gripValues[handIndex] < gripThresholdDeactivate) //user has let go
                {
                    gripStates[handIndex] = GRIP_STATE.OPEN;
                }
                else
                {
					//move the object in relation to controller movement
                    VRGrabbable g = grabbedObjects[handIndex];
                    Rigidbody rb = g.GetComponent<Rigidbody>();

                    Vector3 between = hands[handIndex].grabOffset.position - g.transform.position;
                    Vector3 direction = between.normalized;

                    rb.velocity = between / Time.deltaTime;

                    //also handle controller, object rotation
                    Quaternion betweenRot = hands[handIndex].grabOffset.rotation * Quaternion.Inverse(g.transform.rotation);
                    Vector3 axis;
                    float angle;
                    betweenRot.ToAngleAxis(out angle, out axis);

                    rb.angularVelocity = angle * Mathf.Deg2Rad * axis / Time.deltaTime;
                }
			}
            else //user is not gripping, set either AIR or OBJECT states
			{
                if(gripValues[handIndex] > gripThresholdActivate)
                    if(hands[handIndex].grabbables.Count == 0) //nothing to grab in area
				    {
                        gripStates[handIndex] = GRIP_STATE.AIR;
                        Vector3 handInTracking = transform.worldToLocalMatrix.MultiplyPoint(hands[handIndex].transform.position);

                        gripLocations[handIndex] = handInTracking;
                        cameraRigGripLocation[handIndex] = this.transform.position;
					}
					else //something in area to grab
					{
                        gripStates[handIndex] = GRIP_STATE.OBJECT;
                        grabbedObjects[handIndex] = hands[handIndex].grabbables[0]; //grab first object in list
                        hands[handIndex].grabOffset.transform.position = grabbedObjects[handIndex].transform.position;
                        hands[handIndex].grabOffset.transform.rotation = grabbedObjects[handIndex].transform.rotation;
                    }
			}
    }

    void snapTurn(int handIndex)
    {
	    if (snapStates[handIndex] == SNAP_STATE.WAITING)
	    {
		    float lr = joyValues[handIndex].x;

		    if (Mathf.Abs(lr) > snapThresholdActivate)
		    {
			    snapStates[handIndex] = SNAP_STATE.ACTIVE;
			    float rotateAmount = lr > 0 ? snapDegree : -snapDegree;
			    Vector3 currentFootPosition = getFootPositionWorld();

			    transform.Rotate(0, rotateAmount, 0, Space.Self);
			    doTeleport(currentFootPosition, transform.rotation); //moves back to where we were
		    }
	    }
	    else if (snapStates[handIndex] == SNAP_STATE.ACTIVE)
	    {
		    float lr = joyValues[handIndex].x;

		    if (Mathf.Abs(lr) < snapThresholdDeactivate)
		    {
			    snapStates[handIndex] = SNAP_STATE.WAITING;
		    }
	    }
    }
    

    // Update is called once per frame
    void Update()
    {
        //get values for controller grips
        gripValues[0] = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);
        gripValues[1] = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);
        //get values for controller joysticks
        joyValues[0] = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        joyValues[1] = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        
        for(int i = 0; i < 2; i++)
		{
			//destroy teleporter arc prefab
			foreach (Transform t in teleporterStartPoses[i])
			{
				GameObject.Destroy(t.gameObject);
			}
			
			teleport(i);
			grabbing(i);
			snapTurn(i);
		}

        //move player based on grip states
        if (gripStates[0] == GRIP_STATE.AIR && gripStates[1] == GRIP_STATE.AIR) //both controllers
		{
            this.transform.position = (cameraRigGripLocation[0] + displacements[0] + cameraRigGripLocation[1] + displacements[1]) / 2.0f;
		}
        else if (gripStates[0] == GRIP_STATE.AIR) //left controller
		{
            this.transform.position = cameraRigGripLocation[0] + displacements[0];
		}
		else if (gripStates[1] == GRIP_STATE.AIR) //right controller
		{
            this.transform.position = cameraRigGripLocation[1] + displacements[1];
        }
    }
}

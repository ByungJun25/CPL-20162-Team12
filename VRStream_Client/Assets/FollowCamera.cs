using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

    public GameObject target = null;
    public bool orbitY = false;

    private Vector3 positionOffset = Vector3.zero;


	// Use this for initialization
	private void Start () {
        positionOffset = target.transform.position + transform.position;
	}
	
	// Update is called once per frame
	private void Update () {
        transform.LookAt(target.transform);

        if(orbitY)
            transform.RotateAround(target.transform.position, Vector3.up, Time.deltaTime*15);

        transform.position = target.transform.position + positionOffset;
	}
}

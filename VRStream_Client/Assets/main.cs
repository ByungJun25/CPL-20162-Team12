using UnityEngine;
using System.Collections;

public class main : MonoBehaviour {

    private VRSCameraManager mCameraManger;
    private VRSCameraPreview mCameraPreview;

    private bool mlsOn = true;

    private Ctcpclient mTCPClient;
    VRSMPEG4Encoder mEncoder = new VRSMPEG4Encoder();

    
	// Use this for initialization
	void Start () {
        mCameraManger = new VRSCameraManager();
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

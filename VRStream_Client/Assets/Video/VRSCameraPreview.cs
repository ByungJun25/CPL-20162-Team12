using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VRSCameraPreview : MonoBehaviour {

    private static int WIDTH = 640;
    private static int HEIGHT = 480;
    private static int FPS = 25;

    private WebCamTexture mCamera;
    //private int mFrameLength; bit per pixel을 구하지 못했음.

    private static int MAX_BUFFER = 100;
    private LinkedList<byte[]> mQueue;
    private byte[] mImageData;
    private byte[] mLastFrame;
    private Renderer render;

    // Use this for initialization
    void Start () {
        mCamera = new WebCamTexture();
        mQueue = new LinkedList<byte[]>();

        mCamera.requestedHeight = HEIGHT;
        mCamera.requestedWidth = WIDTH;
        mCamera.requestedFPS = FPS;

        

        render = GetComponent<Renderer>();
        render.material.mainTexture = mCamera;
        mCamera.Play();

    }

    public VRSMPEG4Encoder mEncoder;
	// Update is called once per frame
	void Update () {
	    //if(mEncoder.isRunning)
           // mEncoder.add()   ? data가 머지
	}

    private void resetBuff()
    {
        lock(mQueue)
        {
            mQueue.Clear();
            mLastFrame = null;
        }
    }
    public byte[] getImageBuffer()
    {
        lock(mQueue)
        {
            if (mQueue.Count > 0)
            {
                mLastFrame = mQueue.Last.Value;
                mQueue.RemoveLast();
            }
                
        }
        return mLastFrame;
    }

    public int getPreviewWidth() { return mCamera.width; }
    
    public int getPrerviewHeight() { return mCamera.height; }


}

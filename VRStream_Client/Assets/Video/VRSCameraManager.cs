using UnityEngine;
using System.Collections;

public class VRSCameraManager {

    WebCamTexture mCamera;
    Renderer render;

   
    public VRSCameraManager()
    {
        mCamera = new WebCamTexture();
        
    }

    public WebCamTexture getCamera() { return mCamera; }

    private void stopCamera()
    {
        if(mCamera != null)
        {
            mCamera.Stop();
            mCamera = null;
        }
    }

    public void onPause() { stopCamera(); }

    public void onResume()
    {
        if(mCamera==null)
        {
            mCamera = new WebCamTexture();
        }
    }

   

}

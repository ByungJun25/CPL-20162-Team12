using UnityEngine;
using System.Collections;

public class background : MonoBehaviour {
    WebCamTexture back;
    Renderer render;
	// Use this for initialization
	void Start () {
        back = new WebCamTexture();
        render = GetComponent<Renderer>();
        render.material.mainTexture = back;
        back.Play();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

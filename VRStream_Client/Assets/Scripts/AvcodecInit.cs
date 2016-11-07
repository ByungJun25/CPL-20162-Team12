using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential,Pack =1)]
public struct VideoPakcet
{
    byte data;
    Int64 pts;
    Int64 dts;
    int flags;
}
public class AvcodecInit : MonoBehaviour {
    [DllImport("ffmpeg_dll")]
    private static extern int av_register_all_m();

    [DllImport("ffmpeg_dll")]
    private static extern void avcodec_register_all_m();

    [DllImport("ffmpeg_dll")]
    private static extern int ffmpeg_init();

    [DllImport("ffmpeg_dll")]
    private static extern void avcodec_release();

    [DllImport("ffmpeg_dll")]
    private static extern IntPtr encode_m([In]byte[] image_);

    // Use this for initialization
    void Start () {
        av_register_all_m();
        avcodec_register_all_m();
        

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

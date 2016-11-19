using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
using System;

/*
typedef struct VideoPacket {
	uint8_t* data;
	int64_t pts;
	int64_t dts;
	int flags;
}VP;
*/
[StructLayout(LayoutKind.Sequential,Pack =1)]
public struct VideoPacket
{    
    public byte data;
    public Int64 pts;
    public Int64 dts;
    public int flags;
}

public class prii : MonoBehaviour {

    [DllImport("ffmpeg_dll",CallingConvention =CallingConvention.Cdecl)]
    private static extern int av_register_all_m();

    [DllImport("ffmpeg_dll")]
    private static extern void avcodec_register_all_m();

    [DllImport("ffmpeg_dll", CallingConvention =CallingConvention.Cdecl)]
    private static extern int ffmpeg_init();

    [DllImport("ffmpeg_dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int encode_m(byte[] ch,out IntPtr data,out Int64 pts,out Int64 dts,out int flags,out int size);
    //ref byte data,

    // Use this for initialization
    void Start () {
        av_register_all_m();
        avcodec_register_all_m();
        ffmpeg_init();
        
        //이미지 불러오기
        string filepath = Application.streamingAssetsPath + "/pencils.jpg";
        byte[] filedata = File.ReadAllBytes(filepath);

        IntPtr data = IntPtr.Zero;
        Int64 pts, dts;
        int flags,size;



        int check = encode_m(filedata,out data, out pts, out dts, out flags,out size);
        Debug.Log(check);
        Debug.Log(size);
        byte[] arr_data = new byte[size];
        Marshal.Copy(data, arr_data, 0, size);
        Debug.Log(arr_data[0] + " f " + arr_data[1] + " f " + arr_data[2] + " f " + arr_data[3]);

        //ref data_m,
        //Debug.Log(data_m);


    }
	
	// Update is called once per frame
	void Update () {
	
	}
}

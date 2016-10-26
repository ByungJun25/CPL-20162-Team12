using UnityEngine;
using System.Collections;

public class VRSVideoPacket{

    public int size;
    public int pts;
    public int dts;
    public int flags;
    public byte[] data;

    public float touch_x = -1;

    public VRSVideoPacket(byte[] _data,int _pts, int _dts,int _flags)
    {
        data = _data;
        pts = _pts;
        dts = _dts;
        flags = _flags;
        size = data.Length;
    }

}

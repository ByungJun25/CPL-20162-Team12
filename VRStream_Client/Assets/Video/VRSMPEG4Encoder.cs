using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class VRSMPEG4Encoder {

    private static int MAX_BUFFER = 30;
    private static int SIZE_OF_FRAME = 25;

    private extern int encode(byte[][] images);
    private extern int encode2(byte[] image);
    private extern void release();

    private Queue<VRSVideoPacket> mVideoQueue;

    public void writeEncoded(byte[] encoded, int pts, int dts, int flags)
    {
        VRSVideoPacket pkt = new VRSVideoPacket(encoded, pts, dts, flags);
        
        try
        {
            mVideoQueue.Enqueue(pkt);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public Boolean isRunning = false;

    private LinkedList<byte[]> mQueue;
    ArrayList buffer = new ArrayList();

    public VRSMPEG4Encoder()
    {
        mQueue = new LinkedList<byte[]>();
        mVideoQueue = new Queue<VRSVideoPacket>();

    }

    public void add(byte[] image)
    {
        try
        {
            //clone?
            mQueue.AddLast(image);
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }

    public VRSVideoPacket getEncoded()
    {
        isRunning = true;
        VRSVideoPacket encodedpkt = null;
        try
        {
            encodedpkt = mVideoQueue.Dequeue();
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }

        return encodedpkt;
    }

    protected void awake()
    {
        isRunning = false;
        release();
    }
    
}

using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;


public class Cdudpclient : MonoBehaviour {

    Thread readThread;
    Thread sendThread;

    static int port = 9900;
    static string ser_add = "192.0.0.1";

    Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    static IPAddress ip = IPAddress.Parse(ser_add);
    static IPEndPoint endPoint = new IPEndPoint(ip, port);

    public string lastReceivedPacket = "";
    public string allReceivedPackets = "";

    // start from unity3d
    void Start()
    {
        client.Connect(endPoint);

        // create thread for reading UDP messages
        readThread = new Thread(new ThreadStart(ReceiveData));
        readThread.IsBackground = true;
        readThread.Start();

        sendThread = new Thread(new ThreadStart(SendData));
        sendThread.IsBackground = true;
        sendThread.Start();


    }

    // Unity Update Function
    void Update()
    {
        // check button "s" to abort the read-thread
        if (Input.GetKeyDown("q"))
            stopThread();
    }

    // Unity Application Quit Function
    void OnApplicationQuit()
    {
        stopThread();
    }

    // Stop reading UDP messages
    private void stopThread()
    {
        if (readThread.IsAlive)
        {
            readThread.Abort();
        }
        if (sendThread.IsAlive)
        {
            sendThread.Abort();
        }
        client.Close();
    }
    // send thread function
    private void SendData()
    {
        while(true)
        {
            try
            {
                
                IPEndPoint serverIP = new IPEndPoint(IPAddress.Parse(ser_add), port);
                string test_message = "hello server?";
                byte[] ii = Encoding.UTF8.GetBytes(test_message);
                client.Send(ii, ii.Length, SocketFlags.None);
            }
            catch(Exception err)
            {
                print(err.ToString());
            }
        }
    }
    // receive thread function
    private void ReceiveData()
    {
        
        while (true)
        {
            try
            {
                // receive bytes
                byte[] data = new byte[1024];

                int length = client.Receive(data,0,data.Length,SocketFlags.None);

                // encode UTF8-coded bytes to text format
                string text = Encoding.UTF8.GetString(data);

                // show received message
                print(">> " + text);

                // store new massage as latest message
                lastReceivedPacket = text;

                // update received messages
                allReceivedPackets = allReceivedPackets + text;

            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    // return the latest message
    public string getLatestPacket()
    {
        allReceivedPackets = "";
        return lastReceivedPacket;
    }
}

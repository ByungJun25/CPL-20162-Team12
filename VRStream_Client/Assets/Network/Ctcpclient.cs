using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;
using System.Text;
using System.IO;

public class Ctcpclient : MonoBehaviour {

    
    private Socket client;
    public string ipaddress_string = "127.0.0.1";
    public int port = 9999;


    byte[] data_send, data_receive;
    NetworkStream ns;
    StreamReader sr;
    StreamWriter sw;


    private VRSMPEG4Encoder pEncoder;

	// Use this for initialization
	void Start () {
        IPHostEntry ipHost = Dns.GetHostEntry(ipaddress_string);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        
        client.Connect(ipEndPoint);
        Console.WriteLine("Socket connect : {0}",client.RemoteEndPoint.ToString());

        ns = new NetworkStream(client);
        sr = new StreamReader(ns);
        sw = new StreamWriter(ns);

        byte[] data_receive = new byte[1024];

        pEncoder = new VRSMPEG4Encoder();
        

    }
	
	// Update is called once per frame
	void Update () {
        VRSVideoPacket packet = pEncoder.getEncoded();

        string dataToSend = Console.ReadLine();
        byte[] data = Encoding.Default.GetBytes(dataToSend);

        if(packet != null)
        {
            byte[] headerBytes = new byte[200];
            Array.Clear(headerBytes, 0, headerBytes.Length);

            sw.Flush();
            sw.Write(Convert.ToBase64String(headerBytes));
            sw.Flush();
            sw.Write(Convert.ToBase64String(packet.data));
            sw.Flush();


        }

        int byte_receive = client.Receive(data_receive);

        Console.WriteLine("Server : {0}", Encoding.ASCII.GetString(data_receive, 0, byte_receive));


    }

}

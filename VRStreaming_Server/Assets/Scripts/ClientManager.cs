using UnityEngine;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;

public class ClientManager : MonoBehaviour
{
    /*
     * This is init Variable.
     * clientSocket: Socket for client.
     * buffer: buffer for packet.
     * clientName: Client Name that received from server.
    */
    private Socket clientSocket;
    private byte[] buffer = null;
    private string clientName;
    private int clientNumber;
    private int bytesRec = 0;

    public static event Action<int, string> Disconnected;

    /*
     * This method initialize all of variable in class.
     * It not only initialize but also add function which send message to client to SendMessageEvent event in ServerManager Script. 
     * Run the coroutine method after initialize.
     * coroutine method name is ReceiveVideo.
    */
    public void init(Socket clientSocket, string clientName, int clientNumber, int bufferSize, Vector3 position)
    {
        Debug.Log("Client prefab initialize");
        this.clientSocket = clientSocket;
        this.clientName = clientName;
        this.clientNumber = clientNumber;
        this.transform.position = position;
        buffer = new byte[bufferSize];
        clientSocket.NoDelay = true;
        clientSocket.ReceiveTimeout = 5000;

        StartCoroutine(ReceiveVideo());
    }

    /*
     * This is corountine method. It receives the packet from client who connect to server.
     * It not only get the packet but also show the video on Texture.
     * This method run every frame. 
     * If client disconnect to server, It will close the socket and delete itself(client game object) by itself.
    */
    IEnumerator ReceiveVideo()
    {
        bool isHeaderExtract = false; 

        int data_len = 0;
        int offset = 0;

        byte[] frame = {0};
        byte[] header = { 0, 0, 0, 0 };

        while (true)
        {
            try
            {
                bytesRec = 0;
                bytesRec = clientSocket.Receive(buffer, offset, (buffer.Length - offset), 0);
                offset += bytesRec;
                if (bytesRec <= 0)
                {
                    Debug.Log("Client disconnect: " + clientName);
                    clientSocket.Close();
                    clientSocket = null;
                    Disconnected(clientNumber, clientName);
                    OnDisableSendMessage();
                    Destroy(this.gameObject);
                }
                else
                {
                    if (!isHeaderExtract)
                    {
                        if (offset < 4)
                        {
                            continue;
                        }
                        Buffer.BlockCopy(buffer, 0, header, 0, header.Length);
                        data_len = BitConverter.ToInt32(header, 0);
                        if (buffer.Length < data_len)
                        {
                            Array.Resize<byte>(ref buffer, data_len);
                        }
                        Buffer.BlockCopy(buffer, header.Length, buffer, 0, offset - header.Length);
                        offset -= header.Length;
                        Array.Clear(header, 0, header.Length);
                        isHeaderExtract = true;
                    }
                    else
                    {
                        if (offset >= data_len)
                        {
                            Array.Resize<byte>(ref frame, data_len);
                            Buffer.BlockCopy(buffer, 0, frame, 0, data_len);
                            ShowVideo(frame);
                            Array.Clear(frame, 0, frame.Length);
                            offset -= data_len;
                            Buffer.BlockCopy(buffer, data_len, buffer, 0, offset);
                            isHeaderExtract = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                clientSocket.Close();
                clientSocket = null;
                Disconnected(clientNumber, clientName);
                OnDisableSendMessage();
                Destroy(this.gameObject);
            }
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }

    private void ShowVideo(byte[] frame)
    {
        Texture2D tex = new Texture2D(640, 360);
        tex.LoadImage(frame);
        GetComponent<Renderer>().material.mainTexture = tex;
        tex.Apply();
    }

    /*
     * Put the SendMessageToClient method in sendMessageEvent event.
    */
    public void OnEnableSendMessage()
    {
        ServerManager.sendMessageEvent += SendMessageToClient;
    }

    /*
     * Put the SendMessageToClient method out sendMessageEvent event.
    */
    public void OnDisableSendMessage()
    {
        ServerManager.sendMessageEvent -= SendMessageToClient;
    }

    /*
     * This method send a message from server to client.
    */
    private void SendMessageToClient(string message)
    {
        byte[] b_message = Encoding.UTF8.GetBytes(message);
        clientSocket.Send(b_message);
    }

    public string GetClientName()
    {
        return clientName;
    }
}

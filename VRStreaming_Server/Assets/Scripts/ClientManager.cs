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
    private string clientName ="";
    private int clientNumber;
    private int bytesRec = 0;

    public static event Action<int, string> Disconnected;

    /*
     * This method initialize all of variable in class.
     * It not only initialize but also add function which send message to client to SendMessageEvent event in ServerManager Script. 
     * Run the coroutine method after initialize.
     * coroutine method name is ReceiveVideo.
    */
    public void init(Socket clientSocket, int clientNumber, int bufferSize, Vector3 position)
    {
        Debug.Log("Client prefab initialize");
        buffer = new byte[bufferSize];
        this.clientSocket = clientSocket;
        /*
        int recv_len = clientSocket.Receive(buffer);
        if (recv_len <= 0)
        {
            Debug.Log("fail to get client name");
            return;
        }
        else
        {
            this.clientName = Encoding.Default.GetString(buffer);
            this.clientName = this.clientName.Replace("\0", String.Empty);
            Array.Clear(buffer, 0, buffer.Length);
        }
        if(!string.IsNullOrEmpty(this.clientName.Trim()))
        {
            byte[] b_message = Encoding.UTF8.GetBytes("Start");
            clientSocket.Send(b_message);
        }
        */
        this.clientNumber = clientNumber;
        this.transform.position = position;
        clientSocket.NoDelay = true;
        clientSocket.ReceiveTimeout = 5000;

        ServerManager.sendWaitingMessageEvent += sendWaitingMessage;

        int recv_len = clientSocket.Receive(buffer);
        if (recv_len <= 0)
        {
            Debug.Log("fail to get client name");
            return;
        }
        else
        {
            this.clientName = Encoding.Default.GetString(buffer);
            this.clientName = this.clientName.Replace("\0", String.Empty);
            Array.Clear(buffer, 0, buffer.Length);
        }
        if (!string.IsNullOrEmpty(this.clientName.Trim()))
        {
            byte[] b_message = Encoding.UTF8.GetBytes("Start");
            clientSocket.Send(b_message);
        }

        StartCoroutine(ReceiveVideo());
    }

    private void sendWaitingMessage()
    {
        byte[] b_message = Encoding.UTF8.GetBytes("onesecond");
        clientSocket.Send(b_message);
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
                    OnDisableSendImage();
                    ServerManager.sendWaitingMessageEvent -= sendWaitingMessage;
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
                            StartCoroutine(ShowVideo(frame));
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
                Disconnected(clientNumber, clientName);
                OnDisableSendMessage();
                OnDisableSendImage();
                ServerManager.sendWaitingMessageEvent -= sendWaitingMessage;
                Destroy(this.gameObject);
                clientSocket.Close();
                clientSocket = null;
            }
            yield return null;
        }
    }

    IEnumerator ShowVideo(byte[] frame)
    {
        //Debug.Log(frame.Length);
        Texture2D tex = new Texture2D(640, 360);
        tex.LoadImage(frame);
        GetComponent<Renderer>().material.mainTexture = tex;
        //tex.Apply();
        yield return null;
    }
    /*
    private void ShowVideo(byte[] frame)
    {
        Debug.Log(frame.Length);
        Texture2D tex = new Texture2D(640, 360);
        tex.LoadImage(frame);
        GetComponent<Renderer>().material.mainTexture = tex;
        //tex.Apply();
    }
    */
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

    public void OnEnableSendImage()
    {
        ServerManager.sendImageEvent += sendImageToClient;
    }

    public void OnDisableSendImage()
    {
        ServerManager.sendImageEvent -= sendImageToClient;
    }

    /*
     * This method send a message from server to client.
    */
    private void SendMessageToClient(string message)
    {
        byte[] b_message = Encoding.UTF8.GetBytes(message);
        clientSocket.Send(b_message);
    }

    private void sendImageToClient(byte[] data)
    {
        string msg = "Image";
        byte[] b_message = Encoding.UTF8.GetBytes(msg);
        clientSocket.Send(b_message);

        byte[] imageHeader = new byte[4];

        int image_length = data.Length;

        imageHeader[0] = (byte)(image_length);
        imageHeader[1] = (byte)(image_length >> 8);
        imageHeader[2] = (byte)(image_length >> 16);
        imageHeader[3] = (byte)(image_length >> 24);
        clientSocket.Send(imageHeader);

        int imageDate_Len = image_length + 4;
        byte[] imageDate = new byte[imageDate_Len];
        Buffer.BlockCopy(imageHeader, 0, imageDate, 0, 4);
        Buffer.BlockCopy(data, 0, imageDate, 4, imageHeader.Length);
        clientSocket.Send(data);
    }

    public string GetClientName()
    {
        return clientName;
    }
}

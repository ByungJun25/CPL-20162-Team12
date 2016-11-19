using UnityEngine;
using System.Collections;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.UI;

public class Clientscript : MonoBehaviour
{
    IPAddress ipAddr;
    IPEndPoint ipEndPoint;
    Socket serverSocket;
    AudioSource audioSource;
    public AudioClip audioClip;

    Texture2D texture, tex;
    GameObject imageQuad;

    byte[] textureTojpg;
    byte[] imagePacket;
    public string sockadd = "127.0.0.1";
    byte[] buffer = new byte[1024];
    string str_msg;
    int textureTojpg_len;
    byte[] bytes_textureTojpg_len = new byte[4];
    public string my_name = "USER";

    bool emergency = true;
    bool image_loop = false;
    private Text textVis;

    bool isHeaderExtract = false;

    int data_len = 0;
    int offset = 0;
    int bytesRec = 0;
    byte[] frame = { 0 };
    byte[] header = { 0, 0, 0, 0 };


    void Awake()
    {
        WebCamTexture webcamTexture = new WebCamTexture();
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = webcamTexture;
        webcamTexture.Play();
        texture = new Texture2D(GetComponent<Renderer>().material.mainTexture.width, GetComponent<Renderer>().material.mainTexture.height);
        audioSource = GetComponent<AudioSource>();
        textVis = GameObject.Find("Console").GetComponent<Text>();
        textVis.enabled = false;
   

        tex = new Texture2D(640, 360);
        imageQuad = GameObject.Find("imageQuad");
        imageQuad.SetActive(false);


        try
        {
            ipAddr = IPAddress.Parse(sockadd);
            ipEndPoint = new IPEndPoint(ipAddr, 9090);

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Connect(ipEndPoint);
            

            imagePacket = new byte[1];

            //최초 접속시 이름 사용자 이름 전송
            byte[] name = Encoding.Default.GetBytes(my_name);
            serverSocket.Send(name);

            //연결 완료 메시지 전달받음
            serverSocket.Receive(buffer);
            string recieveF = Encoding.Default.GetString(buffer);
             Array.Clear(buffer, 0, buffer.Length);

            textVis.text = "접속";

            StartCoroutine(Client());


        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }


    IEnumerator Client()
    {

        while (true)
        {
            texture.SetPixels((GetComponent<Renderer>().material.mainTexture as WebCamTexture).GetPixels());
            texture.Apply();
            textureTojpg = texture.EncodeToJPG();
            textureTojpg_len = textureTojpg.Length;
            
            bytes_textureTojpg_len[0] = (byte)(textureTojpg_len);
            bytes_textureTojpg_len[1] = (byte)(textureTojpg_len >> 8);
            bytes_textureTojpg_len[2] = (byte)(textureTojpg_len >> 16);
            bytes_textureTojpg_len[3] = (byte)(textureTojpg_len >> 24);
            if (imagePacket.Length != textureTojpg.Length + 4)
            {
                Array.Resize<byte>(ref imagePacket, textureTojpg.Length + 4);
            }
            Buffer.BlockCopy(bytes_textureTojpg_len, 0, imagePacket, 0, 4);
            Buffer.BlockCopy(textureTojpg, 0, imagePacket, 4, textureTojpg.Length);
            try
            {
                serverSocket.Send(imagePacket);
            }
            catch
            {
                yield break;
            }
            Array.Clear(imagePacket, 0, imagePacket.Length);

            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    void Update()
    {
        if(!serverSocket.Connected)
        {
            if(!emergency)
                textVis.text = "상황종료";
            else 
                textVis.text = "연결이 끊어짐";

            StopAllCoroutines();
            return;
        }
        if(serverSocket.Available == 0)
        {
            return;
        }
        if (image_loop)
        {
            bytesRec = 0;
            bytesRec = serverSocket.Receive(buffer, offset, (buffer.Length - offset), 0);
            offset += bytesRec;
            if (!isHeaderExtract)
            {
                if (offset < 4)
                {
                    return;
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
                    StartCoroutine(ShowImage(frame));
                    Array.Clear(frame, 0, frame.Length);
                    isHeaderExtract = false;
                    image_loop = false;
                    offset = 0;
                    Array.Resize<byte>(ref buffer, 1024);
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }
        }
        else
        {
            bytesRec = serverSocket.Receive(buffer);
            str_msg = Encoding.Default.GetString(buffer);
            str_msg = str_msg.Replace("\0", String.Empty);
            checkMessage(str_msg);
        }


    }

    IEnumerator ShowImage(byte[] frame)
    {

        tex.LoadImage(frame);
        imageQuad.GetComponent<Renderer>().material.mainTexture = tex;
        tex.Apply();
        imageQuad.SetActive(true);

        yield return new WaitForSecondsRealtime(15f);

        imageQuad.SetActive(false);
    }

    IEnumerator ShowMessage(string msg)
    {
        textVis.text = msg;
        textVis.enabled = true;

        yield return new WaitForSecondsRealtime(20f);

        textVis.enabled = false;

    }

    public void checkMessage(string msg)
    {
        msg = msg.ToLower();
        if (msg.Contains("alert"))
        {
            Array.Clear(buffer, 0, buffer.Length);
            audioSource.PlayOneShot(audioClip);
        }
        else if(msg.Contains("vibrate"))
        {
            Array.Clear(buffer, 0, buffer.Length);
            Handheld.Vibrate();
        }
        else if(msg.Contains("exit"))
        {
            Array.Clear(buffer, 0, buffer.Length);
            textVis.text = "상황종료";
            emergency = false;
            serverSocket.Close();
            StopAllCoroutines();
        }
        else if(msg.Contains("image"))
        {
            offset += bytesRec-5;
            Buffer.BlockCopy(buffer, 5, buffer, 0, buffer.Length - 5);
            image_loop = true;
        }
        else
        {
            Array.Clear(buffer, 0, buffer.Length);
            StartCoroutine(ShowMessage(msg));
        }

    }
}
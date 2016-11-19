using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

public class ServerManager : MonoBehaviour
{

    private bool[] isSeats = { true, true, true, true };
    private Vector3[] positions = { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
    private string s_Date = System.DateTime.Now.ToString("yyyy_MM_dd");

    private List<string> chatHistory = new List<string>();
    private string currentMessage = string.Empty;
    private Vector2 scrollPosition = Vector2.zero;
    public int MaximumLength = 100;
    string message = "";
    public int seatNum = 4;

    public string filePath = "Assets/Resources/";

    List<ClientManager> clientList;
    bool[] clientSendMsgToggle = { false, false, false, false };

    /*
     * Public variable. 
     * IP: default value is 127.0.0.1
     * PORT: default value is 9090
     * clientPrefab: Prefab variable. You should assign client prefab here.
    */
    public string ipAddress = "127.0.0.1";
    public int portNumber = 9090;
    public GameObject clientPrefab;

    /*
     * SocketManager: This is Singleton Object. It manages the Socket List.
     * AccessController: This is class that control the connection of client. It works as thread. 
     * ClientManager: This is GameObject. When the client connect to server, it was made by server automatically. It get the packet from client and show the video.
    */
    private SocketManager socketManager;
    private AccessController accessController;
    private ClientManager clientManager;

    /*
     * This is Event for sending a message function.
     * All of client object register their method here.
     * When the event happen, all of the client object act their fucntion which send a message.
    */
    public static event Action<string> sendMessageEvent;

    /*
     * Those variables are for client object.
     * clientName: When the client connect to server, server assign the client name using count. e.g. client[1]
     * bufferSize: This is int value for buffer in ClientManager Object. Now, it is 1000000.
     * count: This is client count. But when client disconnect, it doesn't decrease.
    */
    string clientName = "";
    public int bufferSize = 1024;
    private int clientNumber = 0;

    /*
     * When the server open, it make a SocketManager(singleton) and run the AccessController Class and CheckSocket method.
    */
    void Awake()
    {
        clientList = new List<ClientManager>();
        SeatPosInit(seatNum);
        accessController = new AccessController(ipAddress, portNumber);
        socketManager = SocketManager.GetInstance;

        ClientManager.Disconnected += ClientDisconnected;

        StartCoroutine(CheckSocket());
    }

    private void OnGUI()
    {
        SendMessageToClient();
    }

    /*
     * This method check the client who already connect to server every second.
     * This methoud checkt the Queue in SocketManager every second.
     * If there is a socket in Queue, it make a client GameObject and initialize the client object. 
    */
    private IEnumerator CheckSocket()
    {
        Debug.Log("client Connection Check start");
        WaitForSeconds waitSec = new WaitForSeconds(1);

        while (true)
        {
            Socket clientSocket = socketManager.GetSocket();

            if (clientSocket != null)
            {
                Debug.Log("Client Connect: " + clientSocket.RemoteEndPoint.ToString());
                GameObject client = (GameObject)Instantiate(clientPrefab);
                clientManager = client.GetComponent<ClientManager>();
                clientList.Add(clientManager);
                Vector3 pos = SeekSeat();
                clientName = "Client[" + clientNumber + "]";
                client.transform.name = clientName;
                ClientConnected();
                clientManager.init(clientSocket, clientName, clientNumber, bufferSize, pos);
            }
            yield return waitSec;
        }
    }

    private void ClientConnected()
    {
        string message = "Client connected: " + clientName;
        AddMessageOnHistory(message);
    }

    private void ClientDisconnected(int clientNumber, string clientName)
    {
        string message = "Client disconnected: " + clientName;
        AddMessageOnHistory(message);
        ReturnSeat(clientNumber);
        for(int i = 0; i< clientList.Count; i++)
        {
            if(clientList[i].GetClientName() == clientName)
            {
                clientList.RemoveAt(i);
            }
        }
    }

    /*
     * This method for Event named sendMessageEvent.
     * When user who control the server input the some message on chat, this method send a message from server to client who was conneted.
     * But it didn't realize now, so if you want to use this, you should realize this method
    */
    private void SendMessageToClient()
    {
        if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.99f), Screen.height - (Screen.height * 0.97f), 70, 20), "Client[1]"))
        {
            if (clientList[0] != null && !clientSendMsgToggle[0])
            {
                clientList[0].OnEnableSendMessage();
                clientSendMsgToggle[0] = !clientSendMsgToggle[0];
            }
            else if(clientList[0] != null && clientSendMsgToggle[0])
            {
                clientList[0].OnDisableSendMessage();
                clientSendMsgToggle[0] = !clientSendMsgToggle[0];
            }
        }
        if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.638f), Screen.height - (Screen.height * 0.97f), 70, 20), "Client[2]"))
        {
            if (clientList[1] != null && !clientSendMsgToggle[1])
            {
                clientList[1].OnEnableSendMessage();
                clientSendMsgToggle[1] = !clientSendMsgToggle[1];
            }
            else if (clientList[1] != null && clientSendMsgToggle[1])
            {
                clientList[1].OnDisableSendMessage();
                clientSendMsgToggle[1] = !clientSendMsgToggle[1];
            }
        }
        if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.99f), Screen.height - (Screen.height * 0.470f), 70, 20), "Client[3]"))
        {
            if (clientList[2] != null && !clientSendMsgToggle[2])
            {
                clientList[2].OnEnableSendMessage();
                clientSendMsgToggle[2] = !clientSendMsgToggle[2];
            }
            else if (clientList[2] != null && clientSendMsgToggle[2])
            {
                clientList[2].OnDisableSendMessage();
                clientSendMsgToggle[2] = !clientSendMsgToggle[2];
            }
        }
        if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.638f), Screen.height - (Screen.height * 0.470f), 70, 20), "Client[4]"))
        {
            if (clientList[3] != null && !clientSendMsgToggle[3])
            {
                clientList[3].OnEnableSendMessage();
                clientSendMsgToggle[3] = !clientSendMsgToggle[3];
            }
            else if (clientList[3] != null && clientSendMsgToggle[3])
            {
                clientList[3].OnDisableSendMessage();
                clientSendMsgToggle[3] = !clientSendMsgToggle[3];
            }
        }

        GUI.Box(new Rect(Screen.width - (Screen.width * 0.30f), Screen.height - (Screen.height * 0.97f), (Screen.width * 0.2f), (Screen.height * 0.86f)), s_Date);

        scrollPosition = GUI.BeginScrollView(new Rect(Screen.width - (Screen.width * 0.295f), Screen.height - (Screen.height * 0.93f), (Screen.width * 0.19f), (Screen.height * 0.82f)), scrollPosition, new Rect(0, 0, (Screen.width * 0.08f), (Screen.height * 0.08f)), false, false);
        if (chatHistory.Count > 0)
        {
            for (int i = chatHistory.Count - 1; i >= 0; i--)
            {
                GUILayout.Label(chatHistory[i]);
            }
        }
        GUI.EndScrollView();
        GUI.skin.textField.fontSize = 25;
        GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
        currentMessage = GUI.TextField(new Rect(Screen.width - (Screen.width * 0.29f), Screen.height - (Screen.height * 0.09f), (Screen.width * 0.23f), (Screen.height * 0.07f)), currentMessage, MaximumLength);

        if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.05f), Screen.height - (Screen.height * 0.09f), (Screen.width * 0.04f), (Screen.height * 0.07f)), "Send"))
        {
            SendMessage();
        }
        else if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.09f), Screen.height - (Screen.height * 0.97f), (Screen.width * 0.08f), (Screen.height * 0.16f)), "Ring\nAlarm"))
        {
            message = "Click Send Alert Button";
            SendAlertMsg(message);
        }
        else if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.09f), Screen.height - (Screen.height * 0.79f), (Screen.width * 0.08f), (Screen.height * 0.16f)), "Vibrate\nAlarm"))
        {
            message = "Click Send Alarm Button";
            SendAlarmMsg(message);
        }
        else if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.09f), Screen.height - (Screen.height * 0.62f), (Screen.width * 0.08f), (Screen.height * 0.16f)), "Send\nImage"))
        {
            message = "Click Send Image Button";
            SendImage(message);
        }
        else if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.09f), Screen.height - (Screen.height * 0.44f), (Screen.width * 0.08f), (Screen.height * 0.16f)), "Record\nVideo\n(5 seconds)"))
        {
            message = "Click Record Video Button";
            SaveVideo(message);
        }
        else if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.09f), Screen.height - (Screen.height * 0.27f), (Screen.width * 0.08f), (Screen.height * 0.16f)), "Exit\nEmergency"))
        {
            message = "Click Exit Emergency Button";
            SendExitEmergencyMsg(message);
        }
    }

    private void SendMessage()
    {
        if (!string.IsNullOrEmpty(currentMessage.Trim()))
        {
            if (sendMessageEvent != null)
            {
                sendMessageEvent(currentMessage);
            }
            AddMessageOnHistory(currentMessage);
            currentMessage = string.Empty;
        }
    }

    private void SendAlertMsg(string message)
    {
        if (!string.IsNullOrEmpty(message.Trim()))
        {
            if (sendMessageEvent != null)
            {
                sendMessageEvent("alert");
            }
            AddMessageOnHistory(message);
        }
    }

    private void SendAlarmMsg(string message)
    {
        if (!string.IsNullOrEmpty(message.Trim()))
        {
            if (sendMessageEvent != null)
            {
                sendMessageEvent("vibrate");
            }
            AddMessageOnHistory(message);
        }
    }

    private void SendImage(string message)
    {
        if (!string.IsNullOrEmpty(message.Trim()))
        {
            AddMessageOnHistory(message);
        }
    }

    private void SaveVideo(string message)
    {
        if (!string.IsNullOrEmpty(message.Trim()))
        {
            AddMessageOnHistory(message);
        }
    }

    private void SendExitEmergencyMsg(string message)
    {
        if (!string.IsNullOrEmpty(message.Trim()))
        {
            if (sendMessageEvent != null)
            {
                sendMessageEvent("exit");
            }
            AddMessageOnHistory(message);
        }
    }

    private Vector3 SeekSeat()
    {
        Vector3 seatPos = Vector3.zero;

        for (int i = 0; i < isSeats.Length; i++)
        {
            if (isSeats[i])
            {
                isSeats[i] = !isSeats[i];
                seatPos = positions[i];
                clientNumber = i + 1;
                break;
            }
        }
        return seatPos;
    }

    private void AddMessageOnHistory(string message)
    {
        string msg = System.DateTime.Now.ToString("[hh:mm:ss] ");
        msg += message;
        chatHistory.Add(msg);
        WriteData(msg);
    }

    private void ReturnSeat(int clientNumber)
    {
        isSeats[clientNumber-1] = !isSeats[clientNumber-1];
    }

    private void SeatPosInit(int seatNum)
    {
        for (int i = 0; i < seatNum; i++)
        {
            Vector3 pos = new Vector3(GameObject.FindGameObjectWithTag("Screen" + (i + 1)).transform.position.x, GameObject.FindGameObjectWithTag("Screen" + (i + 1)).transform.position.y, -1);
            positions[i] = pos;
        }
    }

    private void WriteData(string message)
    {
        string FileName = s_Date + ".txt";
        FileStream fileStream = new FileStream(filePath + FileName, FileMode.Append, FileAccess.Write);
        StreamWriter writer = new StreamWriter(fileStream, System.Text.Encoding.Unicode);
        if (!string.IsNullOrEmpty(message.Trim()))
        {
            writer.WriteLine(message);
        }
        writer.Close();
    }

    private void initGUIBUttonStyle()
    {
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        
    }
}

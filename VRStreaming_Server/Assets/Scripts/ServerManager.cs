using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

public class ServerManager : MonoBehaviour
{
    private bool[] isSeats = { true, true, true, true };
    private string[] buttonName = { "Client", "Client", "Client", "Client" };
    private Vector3[] positions = { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
    private string s_Date = System.DateTime.Now.ToString("yyyy_MM_dd");

    private List<string> chatHistory = new List<string>();
    private string currentMessage = string.Empty;
    private Vector2 scrollPosition = Vector2.zero;
    public int MaximumLength = 100;
    string message = "";
    public int seatNum = 4;

    public string filePath = "Assets/Resources/";
    public string ImagePath = "Assets/Resources/";
    public string ImageFileName = "";

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
    public static event Action<byte[]> sendImageEvent;
    public static event Action sendWaitingMessageEvent;

    /*
     * Those variables are for client object.
     * bufferSize: This is int value for buffer in ClientManager Object. Now, it is 1000000.
     * count: This is client count. But when client disconnect, it doesn't decrease.
    */
    public int bufferSize = 1024;
    private int clientNumber = 0;
    ClientManager[] clientList = { null, null, null, null};
    bool[] clientSendMsgToggle = { false, false, false, false };
     
    /*
     * When the server open, it make a SocketManager(singleton) and run the AccessController Class and CheckSocket method.
    */
    void Start()
    {
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
                StartCoroutine(CreateClientScreen(clientSocket));
            }
            yield return waitSec;
        }
    }

    IEnumerator CreateClientScreen(Socket clientSocket)
    {
        Debug.Log("Client Connect: " + clientSocket.RemoteEndPoint.ToString());
        GameObject client = Instantiate(clientPrefab);
        clientManager = client.GetComponent<ClientManager>();
        ClientConnected(clientSocket, clientManager);
        yield return null;
    }

    private void ClientConnected(Socket clientSocket, ClientManager clientManager)
    {
        Vector3 pos = SeekSeat();
        clientManager.init(clientSocket, clientNumber, bufferSize, pos);
        if (sendWaitingMessageEvent != null)
        {
            sendWaitingMessageEvent();
        }
        clientList[clientNumber-1] = clientManager;
        buttonName[(clientNumber-1)] = clientManager.GetClientName()+"(Off)";
        string message = "Client connected: " + clientManager.GetClientName();
        AddMessageOnHistory(message);
    }

    private void ClientDisconnected(int clientNumber, string clientName)
    {
        string message = "Client disconnected: " + clientName;
        AddMessageOnHistory(message);
        ReturnSeat(clientNumber);
        clientList[clientNumber - 1] = null;
        buttonName[clientNumber - 1] = "Client";
        clientSendMsgToggle[clientNumber - 1] = false;
    }

    /*
     * This method for Event named sendMessageEvent.
     * When user who control the server input the some message on chat, this method send a message from server to client who was conneted.
     * But it didn't realize now, so if you want to use this, you should realize this method
    */
    private void SendMessageToClient()
    {
        if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.99f), Screen.height - (Screen.height * 0.97f), (Screen.width * 0.1f), (Screen.height * 0.03f)), buttonName[0]))
        {
            if (clientList[0] != null && !clientSendMsgToggle[0])
            {
                clientList[0].OnEnableSendMessage();
                clientList[0].OnEnableSendImage();
                clientSendMsgToggle[0] = !clientSendMsgToggle[0];
                buttonName[0] = clientList[0].GetClientName() + "(On)";
            }
            else if(clientList[0] != null && clientSendMsgToggle[0])
            {
                clientList[0].OnDisableSendMessage();
                clientList[0].OnDisableSendImage();
                clientSendMsgToggle[0] = !clientSendMsgToggle[0];
                buttonName[0] = clientList[0].GetClientName() + "(Off)";
            }
        }
        if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.638f), Screen.height - (Screen.height * 0.97f), (Screen.width * 0.1f), (Screen.height * 0.03f)), buttonName[1]))
        {
            if (clientList[1] != null && !clientSendMsgToggle[1])
            {
                clientList[1].OnEnableSendMessage();
                clientList[1].OnEnableSendImage();
                clientSendMsgToggle[1] = !clientSendMsgToggle[1];
                buttonName[1] = clientList[1].GetClientName() + "(On)";
            }
            else if (clientList[1] != null && clientSendMsgToggle[1])
            {
                clientList[1].OnDisableSendMessage();
                clientList[1].OnDisableSendImage();
                clientSendMsgToggle[1] = !clientSendMsgToggle[1];
                buttonName[1] = clientList[1].GetClientName() + "(Off)";
            }
        }
        if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.99f), Screen.height - (Screen.height * 0.470f), (Screen.width * 0.1f), (Screen.height * 0.03f)), buttonName[2]))
        {
            if (clientList[2] != null && !clientSendMsgToggle[2])
            {
                clientList[2].OnEnableSendMessage();
                clientList[2].OnEnableSendImage();
                clientSendMsgToggle[2] = !clientSendMsgToggle[2];
                buttonName[2] = clientList[2].GetClientName() + "(On)";
            }
            else if (clientList[2] != null && clientSendMsgToggle[2])
            {
                clientList[2].OnDisableSendMessage();
                clientList[2].OnDisableSendImage();
                clientSendMsgToggle[2] = !clientSendMsgToggle[2];
                buttonName[2] = clientList[2].GetClientName() + "(Off)";
            }
        }
        if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.638f), Screen.height - (Screen.height * 0.470f), (Screen.width * 0.1f), (Screen.height * 0.03f)), buttonName[3]))
        {
            if (clientList[3] != null && !clientSendMsgToggle[3])
            {
                clientList[3].OnEnableSendMessage();
                clientList[3].OnEnableSendImage();
                clientSendMsgToggle[3] = !clientSendMsgToggle[3];
                buttonName[3] = clientList[3].GetClientName() + "(On)";
            }
            else if (clientList[3] != null && clientSendMsgToggle[3])
            {
                clientList[3].OnDisableSendMessage();
                clientList[3].OnDisableSendImage();
                clientSendMsgToggle[3] = !clientSendMsgToggle[3];
                buttonName[3] = clientList[3].GetClientName() + "(Off)";
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
            message = "Send Alert";
            SendAlertMsg(message);
        }
        else if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.09f), Screen.height - (Screen.height * 0.79f), (Screen.width * 0.08f), (Screen.height * 0.16f)), "Vibrate\nAlarm"))
        {
            message = "Send Alarm";
            SendAlarmMsg(message);
        }
        else if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.09f), Screen.height - (Screen.height * 0.62f), (Screen.width * 0.08f), (Screen.height * 0.16f)), "Send\nImage"))
        {
            message = "Send Image";
            SendImage(message);
        }
        else if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.09f), Screen.height - (Screen.height * 0.44f), (Screen.width * 0.08f), (Screen.height * 0.16f)), "Send\nMacro\nMessage"))
        {
            message = "Send Escape";
            SendMacroMessage(message);
        }
        else if (GUI.Button(new Rect(Screen.width - (Screen.width * 0.09f), Screen.height - (Screen.height * 0.27f), (Screen.width * 0.08f), (Screen.height * 0.16f)), "Exit\nEmergency"))
        {
            message = "Send Exit Emergency statement";
            SendExitEmergencyMsg(message);
        }
    }

    private void SendMessage()
    {
        string msg = "";
        if (!string.IsNullOrEmpty(currentMessage.Trim()))
        {
            if (sendMessageEvent != null)
            {
                sendMessageEvent(currentMessage);
                for(int i=0; i<clientSendMsgToggle.Length; i++)
                {
                    if(clientSendMsgToggle[i])
                    {
                        msg = "[" + clientList[i].GetClientName() + "] ";
                        msg += currentMessage;
                        AddMessageOnHistory(msg);
                    }
                }
            }
            else
            {
                AddMessageOnHistory("No people selected.");
            }
            currentMessage = string.Empty;
        }
    }

    private void SendAlertMsg(string message)
    {
        string msg = "";
        if (!string.IsNullOrEmpty(message.Trim()))
        {
            if (sendMessageEvent != null)
            {
                sendMessageEvent("alert");
                for (int i = 0; i < clientSendMsgToggle.Length; i++)
                {
                    if (clientSendMsgToggle[i])
                    {
                        msg = "[" + clientList[i].GetClientName() + "] ";
                        msg += message;
                        AddMessageOnHistory(msg);
                    }
                }
            }
            else
            {
                AddMessageOnHistory("No people selected.");
            }
        }
    }

    private void SendAlarmMsg(string message)
    {
        string msg = "";
        if (!string.IsNullOrEmpty(message.Trim()))
        {
            if (sendMessageEvent != null)
            {
                sendMessageEvent("vibrate");
                for (int i = 0; i < clientSendMsgToggle.Length; i++)
                {
                    if (clientSendMsgToggle[i])
                    {
                        msg = "[" + clientList[i].GetClientName() + "] ";
                        msg += message;
                        AddMessageOnHistory(msg);
                    }
                }
            }
            else
            {
                AddMessageOnHistory("No people selected.");
            }
        }
    }

    private void SendImage(string message)
    {
        string msg = "";
        byte[] imageDate = ReadData(ImagePath, ImageFileName);
        if (sendImageEvent != null)
        {
            sendImageEvent(imageDate);
            for (int i = 0; i < clientSendMsgToggle.Length; i++)
            {
                if (clientSendMsgToggle[i])
                {
                    msg = "[" + clientList[i].GetClientName() + "] ";
                    msg += message;
                    AddMessageOnHistory(msg);
                }
            }
        }
        else
        {
            AddMessageOnHistory("No people selected.");
        }
    }

    private void SendMacroMessage(string message)
    {
        string macro = "당장 탈출하라!";
        string msg = "";
        if (!string.IsNullOrEmpty(message.Trim()))
        {
            if (sendMessageEvent != null)
            {
                sendMessageEvent(macro);
                for (int i = 0; i < clientSendMsgToggle.Length; i++)
                {
                    if (clientSendMsgToggle[i])
                    {
                        msg = "[" + clientList[i].GetClientName() + "] ";
                        msg += message;
                        AddMessageOnHistory(msg);
                    }
                }
            }
            else
            {
                AddMessageOnHistory("No people selected.");
            }
            currentMessage = string.Empty;
        }
    }

    private void SendExitEmergencyMsg(string message)
    {
        string msg = "";
        if (!string.IsNullOrEmpty(message.Trim()))
        {
            if (sendMessageEvent != null)
            {
                sendMessageEvent("exit");
                for (int i = 0; i < clientSendMsgToggle.Length; i++)
                {
                    if (clientSendMsgToggle[i])
                    {
                        msg = "[" + clientList[i].GetClientName() + "] ";
                        msg += message;
                        AddMessageOnHistory(msg);
                    }
                }
            }
            else
            {
                AddMessageOnHistory("No people selected.");
            }
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

    private byte[] ReadData(string path ,string fileName)
    {
        string filePath = path + fileName;
        byte[] fileData = File.ReadAllBytes(filePath);
        return fileData;
    }
}

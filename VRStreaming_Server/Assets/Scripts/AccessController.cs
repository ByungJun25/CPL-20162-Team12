using System.Threading;
using System.Net;
using System.Net.Sockets;

public class AccessController {

    /* 
     * Thread.
    */
    private Thread thread;

    /*
     * Information for setting socket
    */
    string ipAddress;
    int portNumber;

    //socket
    Socket clientSocket;
    Socket sListener;

    /*
     * Default Constructor
     * IP: 127.0.0.1
     * PORT: 9090
     * It start thread(Name is Run) automatically.
    */
    public AccessController()
    {
        this.ipAddress = "127.0.0.1";
        this.portNumber = 9090;
        thread = new Thread(Run);
        thread.Start();
    }

    /*
     * Constructor(PORT)
     * IP:127.0.0.1
     * It start thread(Name is Run) automatically.
    */
    public AccessController(int portNumber)
    {
        this.ipAddress = "127.0.0.1";
        this.portNumber = portNumber;
        thread = new Thread(Run);
        thread.Start();
    }

    /*
     * Constructor(IP, PORT)
     * It start thread(Name is Run) automatically.
    */
    public AccessController(string ipAddress, int portNumber)
    {
        this.ipAddress = ipAddress;
        this.portNumber = portNumber;
        thread = new Thread(Run);
        thread.Start();
    }

    /*
     * Run: Make a socket Listener and wait for client
     * If the client require connetion, it will make a clientSocket and then put it in Queue in SocketManager Instance.  
    */
    void Run()
    {
        IPAddress ipAddr = IPAddress.Parse(ipAddress);
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, portNumber);

        sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sListener.Bind(ipEndPoint);
        sListener.Listen(10);

        while (true)
        {
            clientSocket = sListener.Accept();

            SocketManager.GetInstance.PushSocket(clientSocket);
        }
    }

}

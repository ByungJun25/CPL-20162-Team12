using System.Collections.Generic;
using System.Net.Sockets;

public class SocketManager{

    /*
     * Singleton instance
    */
    private static SocketManager instance;

    /*
     * Queue that hold client socket
    */
    private Queue<Socket> socketQueue;
    
    /*
     * Return singleton instance
    */
    public static SocketManager GetInstance
    {
        get
        {
            if(instance == null)
            {
                instance = new SocketManager();
            }
            return instance;
        }
    }

    /*
     * initialize SocketManager and initialize Queue that hold client socket
    */
    private SocketManager()
    {
        socketQueue = new Queue<Socket>();
    }

    /*
     * Push the Socket in socketQueue
    */
    public void PushSocket(Socket socket)
    {
        socketQueue.Enqueue(socket);
    }

    /*
     * If there is a socket in Queue, return socket
    */
    public Socket GetSocket()
    {
        if(socketQueue.Count > 0)
        {
            return socketQueue.Dequeue();
        }
        else
        {
            return null;
        }
    }
}

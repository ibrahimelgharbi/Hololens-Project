using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class MyNetworkManager : MonoBehaviour
{
    public bool isAtStartup = true;
    NetworkClient myClient;
    void Update()
    {
        if (isAtStartup)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                SetupServer();
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                SetupClient();
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                SetupServer();
                SetupLocalClient();
            }
        }

         byte[] data = Encoding.Default.GetBytes("loool");
         //byte[] data = { 124, 100}; 
         //byte[] dBytes = string str;
         //System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
         //str = enc.GetString(dBytes);
         //Debug.Log(data.ToString());
        
         myClient.SendBytes(data, data.Length,Channels.DefaultReliable);

        

    }
    void OnGUI()
    {
        if (isAtStartup)
        {
            GUI.Label(new Rect(2, 10, 150, 100), "Press S for server");
            GUI.Label(new Rect(2, 30, 150, 100), "Press B for both");
            GUI.Label(new Rect(2, 50, 150, 100), "Press C for client");
        }
    }
    // Create a server and listen on a port
    public void SetupServer()
    {
        NetworkServer.Listen(1523);
        isAtStartup = false;
    }

    // Create a client and connect to the server port
    public void SetupClient()
    {
        myClient = new NetworkClient();
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        myClient.Connect("127.0.0.1", 1523);
        isAtStartup = false;
    }

    // Create a local client and connect to the local server
    public void SetupLocalClient()
    {
        myClient = ClientScene.ConnectLocalServer();
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        isAtStartup = false;
    }
    // client function
    public void OnConnected(NetworkMessage netMsg)
    {
        Debug.Log("Connected to server");
    }


    public class RegisterHostMessage : MessageBase
    {
        public string gameName;
        public string comment;
        public bool passwordProtected;
    }

    public class MasterClient
    {
        public NetworkClient client;

        public const short RegisterHostMsgId = 888;

        public void RegisterHost(string name)
        {
            RegisterHostMessage msg = new RegisterHostMessage();
            msg.gameName = name;
            msg.comment = "test";
            msg.passwordProtected = false;

            client.Send(RegisterHostMsgId, msg);
        }
    }
}

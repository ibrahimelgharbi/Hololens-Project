using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.IO;
using System.Text;



public class Socket : MonoBehaviour
{
    // Use this for initialization
    TcpClient mySocket;
    String Host = "192.168.1.86";
    Int32 Port = 55000;

    void Start()
    {
        mySocket = new TcpClient(Host, Port);

        Debug.Log("socket is set up");
    }

    // Update is called once per frame
    void Update()
    {
        GameObject robot = GameObject.Find("Cube");
        
        try
        {
            // Debug.Log(robotIns.B1Angle);
            String pos = (robot.transform.position).ToString(); 
            Byte[] sendBytes = Encoding.UTF8.GetBytes(pos);/*+ (robotIns.B1Angle).ToString() + "B"
                + (robotIns.B2Angle).ToString() + "C" + (robotIns.B3Angle).ToString() + "D");
            */
            mySocket.GetStream().Write(sendBytes, 0, sendBytes.Length);
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e);
        }
    }

}
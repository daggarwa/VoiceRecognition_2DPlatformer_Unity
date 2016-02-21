using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;

public class UDP_RecoServer : MonoBehaviour
{
	Thread receiveThread;
	UdpClient client;
	public int port = 26000; 
	string strReceiveUDP = "";
	string LocalIP = String.Empty;
	string hostname;
	PlayerControl playerControl;
	Gun gun;

	public void Start()
	{
		Application.runInBackground = true;
		init();  
		playerControl = GetComponent<PlayerControl> (); 
		gun = this.transform.Find ("Gun").GetComponent<Gun> ();
	}

	private void init()
	{
		receiveThread = new Thread( new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();
		hostname = Dns.GetHostName();
		IPAddress[] ips = Dns.GetHostAddresses(hostname);
		if (ips.Length > 0)
		{
			LocalIP = ips[0].ToString();
			Debug.Log(" MY IP : "+LocalIP);
		}
	}

	private  void ReceiveData()
	{
		client = new UdpClient(port);
		while (true)
		{
			try
			{
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Broadcast, port);
				byte[] data = client.Receive(ref anyIP);
				strReceiveUDP = Encoding.UTF8.GetString(data);

				Debug.Log(strReceiveUDP);

				if (strReceiveUDP.Equals("Jump")) {
					Debug.Log("Inside Jump");
					playerControl.jump = true;
				}
				else if(strReceiveUDP.Equals("Right") )
				{
					Debug.Log("Inside Right");
					playerControl.updateExternForce(true);

				}
				else if(strReceiveUDP.Equals("Left") )
				{
					Debug.Log("Inside Left");
					playerControl.updateExternForce(false);

				}
				else if(strReceiveUDP.Equals("Shoot") )
				{
					Debug.Log("Inside Shoot");
					gun.shoot = true;
				}


			}
			catch (Exception err)
			{
				print(err.ToString());
			}
		}
	}

	public string UDPGetPacket()
	{
		return strReceiveUDP;
	}

	void OnDisable()
	{
		if ( receiveThread != null) receiveThread.Abort();
		client.Close();
	}
}
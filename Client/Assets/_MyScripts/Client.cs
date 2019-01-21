using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
	private int MAX_USERS_COUNT = 100;
	private int PORT = 2888;
	private int WEB_PORT = 2555;
	private string SERVER_IP = "127.0.0.1";
	private int BUFFER_SIZE = 1024;

	private bool isRunning;
	private int theHostID;
	private int theConnectionID;
	private byte theChannelID;
	private int theWebHostID;
	private byte error;

	#region Singlton
	public static Client Self { get; private set; }
	private void Awake()
	{
		if ( Self == null )
		{
			Self = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
			return;
		}

	}
	#endregion

	private void Start() => Init();
	private void Update() => MessageUpdate();

	private void Init()
	{
		NetworkTransport.Init();

		ConnectionConfig cc = new ConnectionConfig();
		theChannelID = cc.AddChannel(QosType.Reliable);

		HostTopology hostTopology = new HostTopology(cc , MAX_USERS_COUNT);

		theHostID = NetworkTransport.AddHost(hostTopology , 0);
#if UNITY_WEBGL && !UNITY_EDITOR
		connectionID = NetworkTransport.Connect(hostID , SERVER_IP , WEB_PORT , 0 , out error);
#else
		theConnectionID = NetworkTransport.Connect(theHostID , SERVER_IP , PORT , 0 , out error);
#endif
		Debug.Log(string.Format("Normal Socket: {0} on Port {1}\nWebSocket: {2} on Port {3}" , theHostID , PORT , theWebHostID , theWebHostID));

		isRunning = true;
	}

	private void Shutdown()
	{
		isRunning = false;
		NetworkTransport.Shutdown();
	}

	private BinaryFormatter bf = new BinaryFormatter();
	private void MessageUpdate()
	{
		if ( !isRunning ) return;

		int recHostId;
		int connectionId;
		int channelId;
		byte[] buffer = new byte[BUFFER_SIZE];
		int receivedDataSize;

		NetworkEventType e = NetworkTransport.Receive(out recHostId , out connectionId , out channelId , buffer , BUFFER_SIZE , out receivedDataSize , out error);
		switch ( e )
		{
			case NetworkEventType.Nothing: break;

			case NetworkEventType.DataEvent:
				Message m = (Message)bf.Deserialize(new MemoryStream(buffer));
				OnDataReceive(m , recHostId , connectionId , channelId);
				break;
			case NetworkEventType.ConnectEvent:
				Debug.Log(string.Format("Connected, recHostId={0}, connectionId={1}, channelId={2}, receivedDataSize={3}, error={4}" , recHostId , connectionId , channelId , receivedDataSize , error));
				break;
			case NetworkEventType.DisconnectEvent:
				Debug.Log(string.Format("Disconnected, recHostId={0}, connectionId={1}, channelId={2}, receivedDataSize={3}, error={4}" , recHostId , connectionId , channelId , receivedDataSize , error));
				break;

			default:
			case NetworkEventType.BroadcastEvent:
				Debug.LogError("Unexpected event!");
				break;
		}
	}




	private void OnDataReceive( Message msg , int hostId , int connectionId , int channelId )
	{
		switch ( msg.op )
		{
			case Message.OperationCode.None:
				Debug.Log("Unexpected OP : " + msg.op);
				break;
			case Message.OperationCode.CreateAccountResponse:
				OnCreateAccountResponse((MsgResponse_CreateAccount)msg , hostId , connectionId , channelId);
				break;
			case Message.OperationCode.LoginResponse:
				OnLogInResponse((MsgResponse_Login)msg , hostId , connectionId , channelId);
				break;
			default:
				break;
		}
	}

	public event Action<MsgResponse_CreateAccount> CreateAccountResponseReceived;
	private void OnCreateAccountResponse( MsgResponse_CreateAccount msg , int hostId , int connectionId , int channelId )
	{
		CreateAccountResponseReceived?.Invoke(msg);
	}

	public event Action<MsgResponse_Login> LogInResponseReceived;
	private void OnLogInResponse( MsgResponse_Login msg , int hostId , int connectionId , int channelId )
	{
		LogInResponseReceived?.Invoke(msg);
	}

	private void SendToServer( Message msg )
	{
		byte[] buffer = new byte[BUFFER_SIZE];
		MemoryStream ms = new MemoryStream(buffer);
		bf.Serialize(ms , msg);

		NetworkTransport.Send(theHostID , theConnectionID , theChannelID , buffer , BUFFER_SIZE , out error);
	}



	public void CreateAccountRequest( string text , string text1 , string text2 ) => SendToServer(new MsgRequest_CreateAccount(text , text1 , text2));


	public void LogInRequest( string text , string text1 ) => SendToServer(new MsgRequest_Login(text , text1));

}

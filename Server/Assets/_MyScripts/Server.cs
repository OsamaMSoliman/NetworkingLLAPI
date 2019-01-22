using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
	private int MAX_USERS_COUNT = 100;
	private int PORT = 2888;
	private int WEB_PORT = 2555;
	private int BUFFER_SIZE = 1024;

	private byte theChannelID;
	private bool isRunning;
	private int theHostID;
	private int theWebHostID;
	private byte error;

	private Mongo mongo;

	#region Singlton
	public static Server Self { get; private set; }
	private void Awake()
	{
		if (Self == null)
		{
			Self = this;
			DontDestroyOnLoad(gameObject);
		} else
		{
			Destroy(gameObject);
			return;
		}

	}
	#endregion

	private void Start() { Init(); }
	private void Update() { MessageUpdate(); }

	private void Init()
	{
		mongo = new Mongo();
		Debug.Log(mongo.Init() ? "DB is running" : "DB error!");

		NetworkTransport.Init();

		ConnectionConfig cc = new ConnectionConfig();
		theChannelID = cc.AddChannel(QosType.Reliable);

		HostTopology hostTopology = new HostTopology(cc, MAX_USERS_COUNT);

		theHostID = NetworkTransport.AddHost(hostTopology, PORT);
		theWebHostID = NetworkTransport.AddWebsocketHost(hostTopology, WEB_PORT);
		Debug.Log(string.Format("Normal Socket: {0} on Port {1}\nWebSocket: {2} on Port {3}", theHostID, PORT, theWebHostID, WEB_PORT));

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
		if (!isRunning)
			return;
		byte[] buffer = new byte[BUFFER_SIZE];
		int recHostId;
		int connectionId;
		int channelId;
		int receivedDataSize;

		NetworkEventType e = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, buffer, this.BUFFER_SIZE, out receivedDataSize, out this.error);
		switch (e)
		{
			case NetworkEventType.Nothing:
				break;

			case NetworkEventType.DataEvent:
				Message m = (Message)bf.Deserialize(new MemoryStream(buffer));
				OnDataReceive(m, recHostId, connectionId, channelId);
				break;
			case NetworkEventType.ConnectEvent:
				Debug.Log(string.Format("Connected, recHostId={0}, connectionId={1}, channelId={2}, receivedDataSize={3}, error={4}", recHostId, connectionId, channelId, receivedDataSize, error));
				break;
			case NetworkEventType.DisconnectEvent:
				Debug.Log(string.Format("Disconnected, recHostId={0}, connectionId={1}, channelId={2}, receivedDataSize={3}, error={4}", recHostId, connectionId, channelId, receivedDataSize, error));
				break;

			default:
			case NetworkEventType.BroadcastEvent:
				Debug.LogError("Unexpected event!");
				break;
		}

	}

	private void OnDataReceive(Message msg, int hostId, int connectionId, int channelId)
	{
		switch (msg.op)
		{
			case MessageEnums.OperationCode.None:
				Debug.Log("Unexpected OP : " + msg.op);
				break;
			case MessageEnums.OperationCode.CreateAccountRequest:
				CreateAccountResponse((RequestMsg_CreateAccount)msg, hostId, connectionId);
				break;
			case MessageEnums.OperationCode.LoginRequest:
				LogInResponse((RequestMsg_Login)msg, hostId, connectionId, channelId);
				break;
			default:
				break;
		}
	}



	private void SendToClient(int hostId, int connectionId, Message msg)
	{
		byte[] buffer = new byte[BUFFER_SIZE];
		MemoryStream ms = new MemoryStream(buffer);
		bf.Serialize(ms, msg);

		//I knew, theHostID is 0, from testing and Debug.log ,, theWebHostID return 65534
		NetworkTransport.Send(hostId == 0 ? theHostID : theWebHostID, connectionId, theChannelID, buffer, BUFFER_SIZE, out error);
	}

	private void CreateAccountResponse(RequestMsg_CreateAccount msg, int hostId, int connectionId)
	{
		Debug.Log(string.Format("RequestMsg_CreateAccount: {0},{1},{2}", msg.Username, msg.Password, msg.Email));
		MessageEnums.Status s = mongo.InsertAccount(msg.Username, msg.Password, msg.Email);
		ResponseMsg_CreateAccount responseMsg = new ResponseMsg_CreateAccount(s);
		SendToClient(hostId, connectionId, responseMsg);
	}

	private void LogInResponse(RequestMsg_Login msg, int hostId, int connectionId, int channelId)
	{
		Debug.Log(string.Format("{0},{1}", msg.EmailOrUsername, msg.Password));
		string[] data = msg.EmailOrUsername.Split('#');
		string token = Utilities.GenerateRandom(64);
		Account account = mongo.LogIn(msg.EmailOrUsername, msg.Password, connectionId, token);
		if (account == null)
			return;
		SendToClient(hostId, connectionId, new ResponseMsg_Login(account.Status, account.Username, account.Discriminator, account.Token));
	}
}

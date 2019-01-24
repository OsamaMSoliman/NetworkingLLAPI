using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour {
	private int MAX_USERS_COUNT = 100;
	private int PORT = 2888;
	private int WEB_PORT = 2555;
	private string SERVER_IP = "127.0.0.1";
	private int BUFFER_SIZE = 1024;

	#region Timeout
	public event Action TimeOut;
	private float timeOut = 10f;
	private float t;
	private bool waiting;
	private void StartWaiting() {
		t = 0f;
		waiting = true;
	}
	private void StopWaiting(bool success = false) {
		waiting = false;
		if (!success)
			TimeOut?.Invoke();
	}
	private void CheckTimeout() {
		if (waiting) {
			if (t <= timeOut)
				t += Time.deltaTime;
			else
				StopWaiting(false);
		}
	}
	#endregion

	private bool isRunning;
	private int theHostID;
	private int theConnectionID;
	private byte theChannelID;
	private int theWebHostID;
	private byte error;
	public Info Info { get; set; }
	private string Token { get; set; }

	#region Singlton
	public static Client Self { get; private set; }
	private void Awake() {
		if (Self == null) {
			Self = this;
			DontDestroyOnLoad(gameObject);
		} else {
			Destroy(gameObject);
			return;
		}

	}
	#endregion

	private void Start() => Init();
	private void Update() => MessageUpdate();

	private void Init() {
		NetworkTransport.Init();

		ConnectionConfig cc = new ConnectionConfig();
		theChannelID = cc.AddChannel(QosType.Reliable);

		HostTopology hostTopology = new HostTopology(cc, MAX_USERS_COUNT);

		theHostID = NetworkTransport.AddHost(hostTopology, 0); // removing the 0 gives an error,It's OK, I need a random port and 0 does that
#if UNITY_WEBGL && !UNITY_EDITOR
		connectionID = NetworkTransport.Connect(hostID , SERVER_IP , WEB_PORT , 0 , out error);
#else
		theConnectionID = NetworkTransport.Connect(theHostID, SERVER_IP, PORT, 0, out error);
#endif
		Debug.Log(string.Format("Normal Socket: {0} on Port {1}\nWebSocket: {2} on Port {3}", theHostID, PORT, theWebHostID, theWebHostID));

		isRunning = true;
	}

	private void Shutdown() {
		isRunning = false;
		NetworkTransport.Shutdown();
	}

	private BinaryFormatter bf = new BinaryFormatter();
	private void MessageUpdate() {
		if (!isRunning)
			return;
		CheckTimeout();

		byte[] buffer = new byte[BUFFER_SIZE];

		NetworkEventType e = NetworkTransport.Receive(out int recHostId, out int connectionId, out int channelId, buffer, BUFFER_SIZE, out int receivedDataSize, out error);
		switch (e) {
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




	#region Receive Data
	private void OnDataReceive(Message msg, int hostId, int connectionId, int channelId) {
		StopWaiting(true);
		Debug.Log("Data: " + msg.op);
		switch (msg.op) {
			case MessageEnums.OperationCode.None:
				Debug.Log("Unexpected OP : " + msg.op);
				break;
			case MessageEnums.OperationCode.CreateAccountResponse:
				OnCreateAccountResponse((ResponseMsg_CreateAccount)msg);
				break;
			case MessageEnums.OperationCode.LoginResponse:
				OnLogInResponse((ResponseMsg_Login)msg, connectionId);
				break;
			case MessageEnums.OperationCode.FollowAddRemoveResponse:
				OnFollowResponse((ResponseMsg_FollowAddRemove)msg);
				break;
			default:
				break;
		}
	}

	public event Action<ResponseMsg_FollowAddRemove> FollowResponseReceived;
	private void OnFollowResponse(ResponseMsg_FollowAddRemove msg) => FollowResponseReceived?.Invoke(msg);

	public event Action<ResponseMsg_CreateAccount> CreateAccountResponseReceived;
	private void OnCreateAccountResponse(ResponseMsg_CreateAccount msg) => CreateAccountResponseReceived?.Invoke(msg);

	public event Action<ResponseMsg_Login> LogInResponseReceived;
	private void OnLogInResponse(ResponseMsg_Login msg, int connectionId) {
		if (msg.Status == MessageEnums.Status.LoggedIn) {
			Info = new Info(msg, connectionId);
			Info.Email = msg.Email;
			Token = msg.Token;
			SceneManager.LoadSceneAsync("Hub");
		}
		LogInResponseReceived?.Invoke(msg);
	}
	#endregion

	#region Send Request
	private void SendToServer(Message msg) {
		byte[] buffer = new byte[BUFFER_SIZE];
		MemoryStream ms = new MemoryStream(buffer);
		bf.Serialize(ms, msg);

		NetworkTransport.Send(theHostID, theConnectionID, theChannelID, buffer, BUFFER_SIZE, out error);
		StartWaiting();
	}

	public string RequestCreateAccount(string username, string password, string email) {
		if (!Utilities.IsUsername(username))
			return "Invalid Username";
		if (!Utilities.IsEmail(email))
			return "Invalid Email";
		if (string.IsNullOrEmpty(password))
			return "Invalid password";
		SendToServer(new RequestMsg_CreateAccount(username, Utilities.SHA256(password), email));
		return null;
	}

	public string RequestLogIn(string usernameOrEmail, string password) {
		Utilities.StringTpye x = Utilities.TestString(usernameOrEmail);
		if (x == Utilities.StringTpye.Undefined || x == Utilities.StringTpye.Username)
			//if (!Utilities.IsUsernameAndDiscriminator(usernameOrEmail) && !Utilities.IsEmail(usernameOrEmail))
			return "Invalid Username#Discriminator or Email";
		if (string.IsNullOrEmpty(password))
			return "Invalid password";
		SendToServer(new RequestMsg_Login(usernameOrEmail, Utilities.SHA256(password)));
		return null;
	}


	public void RequestAddRemoveFollow(bool unFollow, string UsernameDiscriminatorOrEmail) {
		Utilities.StringTpye x = Utilities.TestString(UsernameDiscriminatorOrEmail);
		switch (x) {
			case Utilities.StringTpye.UsernameAndDiscriminator:
				if (UsernameDiscriminatorOrEmail != Info.Username + '#' + Info.Discriminator)
					SendToServer(new RequestMsg_FollowAddRemove(unFollow, Token, UsernameDiscriminatorOrEmail, false));
				break;
			case Utilities.StringTpye.Email:
				if (UsernameDiscriminatorOrEmail != Info.Email)
					SendToServer(new RequestMsg_FollowAddRemove(unFollow, Token, UsernameDiscriminatorOrEmail, true));
				break;

			default:
			case Utilities.StringTpye.Username:
			case Utilities.StringTpye.Undefined:
				//TODO: ERROR!
				break;
		}
	}


	public void RequestListOfFollows() => SendToServer(new RequestMsg_FollowList(Token));
	#endregion
}

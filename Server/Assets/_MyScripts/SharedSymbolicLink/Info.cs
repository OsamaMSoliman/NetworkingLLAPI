[System.Serializable]
public class Info {
	public int ActiveConnection { get; set; }
	public string Username { get; set; }
	public string Discriminator { get; set; }
	public string Email { get; set; }
	public MessageEnums.Status Status { get; set; }

	public Info(Message msg, int connectionId) {
		if (msg is ResponseMsg_Login) {
			ResponseMsg_Login m = (ResponseMsg_Login)msg;
			ActiveConnection = connectionId;
			Username = m.Username;
			Discriminator = m.Discriminator;
			Status = m.Status;
		}
	}

	public Info(int activeConnection, string username, string discriminator, MessageEnums.Status status) {
		this.ActiveConnection = activeConnection;
		this.Username = username;
		this.Discriminator = discriminator;
		this.Status = status;
	}

	public override string ToString() {
		return string.Format("ActiveConnection: {0}, Username: {1}, Discriminator: {2}, Email: {3}, Status: {4}", ActiveConnection, Username, Discriminator, Email, Status);
	}
}

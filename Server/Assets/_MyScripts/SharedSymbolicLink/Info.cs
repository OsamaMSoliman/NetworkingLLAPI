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

	public Info(int activeConnection, string username, string discriminator, string email, MessageEnums.Status status) {
		this.ActiveConnection = activeConnection;
		this.Username = username;
		this.Discriminator = discriminator;
		this.Email = email;
		this.Status = status;
	}

	public override string ToString() { return string.Format("ActiveConnection: {0}, Username: {1}, Discriminator: {2}, Email: {3}, Status: {4}", ActiveConnection, Username, Discriminator, Email, Status); }

	public override bool Equals(object obj) {
		Info info = obj as Info;
		if (info == null)
			return false;
		return this.Email == info.Email || this.Username + '#' + this.Discriminator == info.Username + '#' + info.Discriminator;
	}
}

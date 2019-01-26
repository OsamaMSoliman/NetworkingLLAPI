[System.Serializable]
public class PublicInfo {
	public string Username { get; set; }
	public string Discriminator { get; set; }
	public string Email { get; set; }
	public MessageEnums.AccountStatus Status { get; set; }

	public PublicInfo(Message msg, int connectionId) {
		if (msg is ResponseMsg_Login) {
			ResponseMsg_Login m = (ResponseMsg_Login)msg;
			Username = m.Username;
			Discriminator = m.Discriminator;
			Status = m.Status == MessageEnums.Status.OK ? MessageEnums.AccountStatus.Online : MessageEnums.AccountStatus.Offline;
		}
	}

	public PublicInfo(string username, string discriminator, string email, MessageEnums.AccountStatus status) {
		this.Username = username;
		this.Discriminator = discriminator;
		this.Email = email;
		this.Status = status;
	}

	public override string ToString() { return string.Format("Username: {0}, Discriminator: {1}, Email: {2}, Status: {3}", Username, Discriminator, Email, Status); }

	//public override bool Equals(object obj) {
	//	PublicInfo info = obj as PublicInfo;
	//	if (info == null)
	//		return false;
	//	return this.Email == info.Email || this.Username + '#' + this.Discriminator == info.Username + '#' + info.Discriminator;
	//}

}

public class Account {
	// needed by MongoDB (if changed _id to anything (ex: ID), MongoDB will create _id on it's own and ignore ID)
	public MongoDB.Bson.ObjectId _id;

	public Account(string username, string password, string email, string Discriminator) {
		this.Username = username;
		this.Password = password;
		this.Email = email;
		this.Discriminator = Discriminator;
	}

	public string Username { get; private set; }
	public string Password { get; private set; }
	public string Email { get; private set; } // unique
	public string Discriminator { get; private set; } // unique (TODO: make it unique when combined with username)

	public int HostId { get; set; }
	public int ConnectionId { get; set; }
	public MessageEnums.AccountStatus Status { get; set; }
	public string Token { get; set; }
	public System.DateTime LastSeen { get; set; }


	public PublicInfo GetPublicInfo() { return new PublicInfo(this.Username, this.Discriminator, this.Email, this.Status); }

	public override string ToString() { return string.Format("Username: {0}, Discriminator: {1}, Email: {2}, Status: {3}", Username, Discriminator, Email, Status); }

}

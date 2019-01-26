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
	public string Discriminator { get; private set; } // unique

	public int ActiveConnection { get; set; }
	public MessageEnums.Status Status { get; set; } // not sure yet about this
	public string Token { get; set; }
	public System.DateTime LastLogin { get; set; }


	public Info GetInfo() { return new Info(this.ActiveConnection, this.Username, this.Discriminator, this.Email, this.Status); }

}

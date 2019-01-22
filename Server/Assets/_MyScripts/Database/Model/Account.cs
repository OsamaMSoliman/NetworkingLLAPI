public class Account
{
	// needed by MongoDB (if changed _id to anything (ex: ID), MongoDB will create _id on it's own and ignore ID)
	public MongoDB.Bson.ObjectId _id;

	public Account(string username, string password, string email)
	{
		this.Username = username;
		this.Password = password;
		this.Email = email;
	}

	public string Username { get; set; }
	public string Password { get; set; }
	public string Email { get; set; } // unique
	public string Discriminator { get; set; } // unique

	public int ActiveConnection { get; set; }
	public MessageEnums.Status Status { get; set; } // not sure yet about this
	public string Token { get; set; }
	public System.DateTime LastLogin { get; set; }


}

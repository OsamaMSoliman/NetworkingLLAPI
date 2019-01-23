using MongoDB.Driver;
using MongoDB.Driver.Builders;

public class Mongo {
	private const string MONGO_URI = "mongodb://server:server1@ds163054.mlab.com:63054/lobbydb";
	private const string DATABASE_NAME = "lobbydb";
	private const string ACCOUNTS_COLLECTION_NAME = "accounts";
	private const string FOLLOWERSHIP_COLLECTION_NAME = "followership";
	private MongoClient mongoClient;
	private MongoServer mongoServer;
	private MongoDatabase mongoDb;
	private MongoCollection<Account> accountsCollection;
	private MongoCollection<Followership> followershippCollection;

	public bool Init() {
		try {
			mongoClient = new MongoClient(MONGO_URI);
			mongoServer = mongoClient.GetServer();
			mongoDb = mongoServer.GetDatabase(DATABASE_NAME);
			accountsCollection = mongoDb.GetCollection<Account>(ACCOUNTS_COLLECTION_NAME);
			followershippCollection = mongoDb.GetCollection<Followership>(FOLLOWERSHIP_COLLECTION_NAME);
			return true;
		} catch (System.Exception) { return false; }
	}

	public void Shutdown() {
		mongoClient = null;
		mongoServer.Shutdown();
		mongoDb = null;
	}


	#region Insert
	// A.K.A Create new Account
	public MessageEnums.Status InsertAccount(string username, string password, string email) {
		if (!Utilities.IsUsername(username))
			return MessageEnums.Status.InvalidUsername;
		if (!Utilities.IsEmail(email))
			return MessageEnums.Status.InvalidEmail;
		if (SelectAccount(email, a => a.Email) != null)
			return MessageEnums.Status.EmailAlreayExists;

		accountsCollection.Insert(new Account(username, password, email, accountsCollection.Count().ToString("0000")));
		return MessageEnums.Status.OK;
	}

	public Info InsertFollowership(string token, string email) { return InsertFollowership(token, SelectAccount(email, a => a.Email)); }

	public Info InsertFollowership(string token, string username, string discriminator) { return InsertFollowership(token, SelectAccount(username, discriminator)); }

	private Info InsertFollowership(string token, Account account) {
		Followership followership = new Followership(new MongoDBRef(ACCOUNTS_COLLECTION_NAME, SelectAccount(token, a => a.Token)._id), new MongoDBRef(ACCOUNTS_COLLECTION_NAME, account._id));
		if (followership.Initiator == followership.Target)
			return null;
		var query = Query.And(Query<Followership>.EQ(f => f.Target, followership.Target), Query<Followership>.EQ(f => f.Initiator, followership.Initiator));
		if (followershippCollection.FindOne(query) == null)
			followershippCollection.Insert(query);
		return account.GetInfo();
	}
	#endregion

	#region Update
	public Account LogIn(string usernameOrEmail, string password, int connectionId, string token) {
		Account account = null;
		string[] data = usernameOrEmail.Split('#');

		if (data.Length > 1 && !string.IsNullOrEmpty(data[1]))
			account = SelectAccountWithPassword(data[0], data[1], password);
		else if (Utilities.IsEmail(usernameOrEmail))
			account = SelectAccountWithPassword(usernameOrEmail, password);

		if (account != null) {
			account.ActiveConnection = connectionId;
			account.Token = token;
			account.Status = MessageEnums.Status.LoggedIn;
			account.LastLogin = System.DateTime.Now;
			accountsCollection.Update(Query<Account>.EQ(a => a.Email, account.Email), Update<Account>.Replace(account));
		}
		return account;
	}
	#endregion

	#region Select
	public Account SelectAccountWithPassword(string email, string password) { return accountsCollection.FindOne(Query.And(Query<Account>.EQ(account => account.Email, email), Query<Account>.EQ(account => account.Password, password))); }
	public Account SelectAccountWithPassword(string username, string discriminator, string password) { return accountsCollection.FindOne(Query.And(Query<Account>.EQ(account => account.Username, username), Query<Account>.EQ(account => account.Discriminator, discriminator), Query<Account>.EQ(account => account.Password, password))); }

	public Account SelectAccount(string Username, string Discriminator) { return accountsCollection.FindOne(Query.And(Query<Account>.EQ(account => account.Username, Username), Query<Account>.EQ(account => account.Discriminator, Discriminator))); }
	public Account SelectAccount(string withThis, System.Linq.Expressions.Expression<System.Func<Account, string>> byThis) { return accountsCollection.FindOne(Query<Account>.EQ(byThis, withThis)); }

	#endregion

	#region Delete

	#endregion
}

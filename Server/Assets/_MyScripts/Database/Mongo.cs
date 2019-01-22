using MongoDB.Driver;
using MongoDB.Driver.Builders;

public class Mongo
{
	private const string MONGO_URI = "mongodb://server:server1@ds163054.mlab.com:63054/lobbydb";
	private const string DATABASE_NAME = "lobbydb";
	private const string COLLECTION_NAME = "accounts";
	private MongoClient mongoClient;
	private MongoServer mongoServer;
	private MongoDatabase mongoDb;
	private MongoCollection<Account> accountsCollection;

	public bool Init()
	{
		try
		{
			mongoClient = new MongoClient(MONGO_URI);
			mongoServer = mongoClient.GetServer();
			mongoDb = mongoServer.GetDatabase(DATABASE_NAME);
			accountsCollection = mongoDb.GetCollection<Account>(COLLECTION_NAME);
			return true;
		} catch (System.Exception) { return false; }
	}

	public void Shutdown()
	{
		mongoClient = null;
		mongoServer.Shutdown();
		mongoDb = null;
	}


	// A.K.A Create new Account
	public MessageEnums.Status InsertAccount(string username, string password, string email)
	{
		if (!Utilities.IsUsername(username))
			return MessageEnums.Status.InvalidUsername;
		if (!Utilities.IsEmail(email))
			return MessageEnums.Status.InvalidEmail;
		if (SelectAccount(email) != null)
			return MessageEnums.Status.EmailAlreayExists;

		Account account = new Account(username, password, email);
		account.Discriminator = accountsCollection.Count().ToString("0000");
		accountsCollection.Insert(account);
		return MessageEnums.Status.OK;
	}

	public Account LogIn(string usernameOrEmail, string password, int connectionId, string token)
	{
		Account account = null;
		string[] data = usernameOrEmail.Split('#');

		if (data.Length > 1 && !string.IsNullOrEmpty(data[1]))
			account = SelectAccount(data[0], data[1], password);
		else if (Utilities.IsEmail(usernameOrEmail))
			account = SelectAccount(usernameOrEmail, password);

		if (account != null)
		{
			account.ActiveConnection = connectionId;
			account.Token = token;
			account.Status = MessageEnums.Status.LoggedIn;
			account.LastLogin = System.DateTime.Now;
			accountsCollection.Update(Query<Account>.EQ(a => a.Email, account.Email), Update<Account>.Replace(account));
		}
		return account;
	}

	public Account SelectAccount(string email) { return accountsCollection.FindOne(Query<Account>.EQ(account => account.Email, email)); }
	public Account SelectAccount(string email, string password) { return accountsCollection.FindOne(Query.And(Query<Account>.EQ(account => account.Email, email), Query<Account>.EQ(account => account.Password, password))); }
	public Account SelectAccount(string username, string discriminator, string password) { return accountsCollection.FindOne(Query.And(Query<Account>.EQ(account => account.Username, username), Query<Account>.EQ(account => account.Discriminator, discriminator), Query<Account>.EQ(account => account.Password, password))); }

	//public bool UpdateAccount() { }
	//public bool DeleteAccount() { }
}

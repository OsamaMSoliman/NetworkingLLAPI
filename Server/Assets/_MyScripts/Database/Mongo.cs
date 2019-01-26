using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
		} catch (Exception) { return false; }
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

	public PublicInfo InsertFollowership(string token, string email) { return InsertFollowership(token, SelectAccount(email, a => a.Email)); }

	public PublicInfo InsertFollowership(string token, string username, string discriminator) { return InsertFollowership(token, SelectAccount(username, discriminator)); }

	private PublicInfo InsertFollowership(string token, Account account) {
		if (account == null)
			return null;
		Followership followership = new Followership(new MongoDBRef(ACCOUNTS_COLLECTION_NAME, SelectAccount(token, a => a.Token)._id), new MongoDBRef(ACCOUNTS_COLLECTION_NAME, account._id));
		if (followership.Initiator == followership.Target)
			return null;
		var query = Query.And(Query<Followership>.EQ(f => f.Target, followership.Target), Query<Followership>.EQ(f => f.Initiator, followership.Initiator));
		if (followershippCollection.FindOne(query) == null)
			followershippCollection.Insert(query);
		return account.GetPublicInfo();
	}
	#endregion

	#region Update
	public Account LogIn(string usernameOrEmail, string password, int hostId, int connectionId, string token) {
		Account account = null;
		string[] data = usernameOrEmail.Split('#');

		if (data.Length > 1 && !string.IsNullOrEmpty(data[1]))
			account = SelectAccountWithPassword(data[0], data[1], password);
		else if (Utilities.IsEmail(usernameOrEmail))
			account = SelectAccountWithPassword(usernameOrEmail, password);

		if (account != null) {
			account.HostId = hostId;
			account.Token = token;
			account.ConnectionId = connectionId;
			account.Status = MessageEnums.AccountStatus.Online;
			account.LastSeen = DateTime.Now;
			accountsCollection.Update(Query<Account>.EQ(a => a.Email, account.Email), Update<Account>.Replace(account));
		}
		return account;
	}

	public PublicInfo ClearAccount(int connectionId) {
		Account account = SelectAccount(connectionId, a => a.ConnectionId);
		if (account == null)
			return null;
		account.Token = null;
		account.ConnectionId = 0; // nobody is ever on 0, when it starts it does that from 1, never 0
		account.Status = MessageEnums.AccountStatus.Offline;
		account.LastSeen = DateTime.Now;
		accountsCollection.Update(Query<Account>.EQ(a => a.Email, account.Email), Update<Account>.Replace(account));
		return account.GetPublicInfo();
	}

	public void UpdateDisconnected(int connectionId) {
		// connectionId: nobody is ever on 0, when it starts it does that from 1, never 0
		var update = Update<Account>.Set(a => a.Token, null).Set(a => a.ConnectionId, 0).Set(a => a.Status, MessageEnums.AccountStatus.Offline);
		var query = Query<Account>.EQ(a => a.ConnectionId, connectionId);
		accountsCollection.Update(query, update);
	}
	#endregion

	#region Select
	public Account SelectAccountWithPassword(string email, string password) { return accountsCollection.FindOne(Query.And(Query<Account>.EQ(account => account.Email, email), Query<Account>.EQ(account => account.Password, password))); }
	public Account SelectAccountWithPassword(string username, string discriminator, string password) { return accountsCollection.FindOne(Query.And(Query<Account>.EQ(account => account.Username, username), Query<Account>.EQ(account => account.Discriminator, discriminator), Query<Account>.EQ(account => account.Password, password))); }

	public Account SelectAccount(string Username, string Discriminator) { return accountsCollection.FindOne(Query.And(Query<Account>.EQ(account => account.Username, Username), Query<Account>.EQ(account => account.Discriminator, Discriminator))); }
	public Account SelectAccount<T>(T withThis, Expression<Func<Account, T>> byThis) { return accountsCollection.FindOne(Query<Account>.EQ(byThis, withThis)); }

	public List<PublicInfo> SelectAllPublicInfoForInitiator(string token) {
		List<PublicInfo> result = new List<PublicInfo>();
		var initiator = new MongoDBRef(ACCOUNTS_COLLECTION_NAME, SelectAccount(token, a => a.Token)._id);
		var queryOfAllFollowershipsWithThisInitiator = Query<Followership>.EQ(f => f.Initiator, initiator);
		foreach (var followership in followershippCollection.Find(queryOfAllFollowershipsWithThisInitiator)) {
			result.Add(SelectAccount(followership.Target.Id.AsObjectId, a => a._id).GetPublicInfo());
		}
		return result;
	}

	public List<Account> SelectAllAccountForTarget(string email) {
		List<Account> result = new List<Account>();
		var target = new MongoDBRef(ACCOUNTS_COLLECTION_NAME, SelectAccount(email, a => a.Email)._id);
		var queryOfAllFollowershipsWithThisTarget = Query<Followership>.EQ(f => f.Target, target);
		foreach (var followership in followershippCollection.Find(queryOfAllFollowershipsWithThisTarget)) {
			result.Add(SelectAccount(followership.Initiator.Id.AsObjectId, a => a._id));
		}
		return result;
	}

	//public List<Account> SelectAllAccountsByFollowership(string token, Expression<Func<Followership, MongoDBRef>> expressionFrom, Func<Followership, MongoDBRef> functionTo) {
	//	List<Account> result = new List<Account>();
	//	var query = Query<Followership>.EQ(expressionFrom, new MongoDBRef(ACCOUNTS_COLLECTION_NAME, SelectAccount(token, a => a.Token)._id));
	//	foreach (var followership in followershippCollection.Find(query)) {
	//		result.Add(SelectAccount(functionTo(followership).Id.AsObjectId, a => a._id));
	//	}
	//	return result;
	//}

	#endregion

	#region Delete
	public void DeleteFollowerShip(string token, string email) { DeleteFollowerShip(token, SelectAccount(email, a => a.Email)); }

	public void DeleteFollowerShip(string token, string username, string discriminator) { DeleteFollowerShip(token, SelectAccount(username, discriminator)); }

	private void DeleteFollowerShip(string token, Account account) {
		if (account == null)
			return;
		var Initiator = new MongoDBRef(ACCOUNTS_COLLECTION_NAME, SelectAccount(token, a => a.Token)._id);
		var Target = new MongoDBRef(ACCOUNTS_COLLECTION_NAME, account._id);
		Followership followership = followershippCollection.FindOne(Query.And(Query<Followership>.EQ(f => f.Initiator, Initiator), Query<Followership>.EQ(f => f.Target, Target)));
		followershippCollection.Remove(Query<Followership>.EQ(f => f._id, followership._id));
	}
	#endregion
}

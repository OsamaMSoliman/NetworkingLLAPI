using MongoDB.Driver;

public class Mongo
{
	private const string MONGO_URI = "";
	private const string DATABASE_NAME = "";
	private MongoClient mongoClient;
	private MongoServer mongoServer;
	private MongoDatabase mongoDb;

	public void Init()
	{
		mongoClient = new MongoClient(MONGO_URI);

	}

	public void Shutdown()
	{
		mongoClient = null;
		mongoServer.Shutdown();
		mongoDb = null;
	}
}

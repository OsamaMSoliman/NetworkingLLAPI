using MongoDB.Bson;
using MongoDB.Driver;

public class Followership {
	public ObjectId _id;
	public MongoDBRef Initiator { get; private set; }
	public MongoDBRef Target { get; private set; }

	public Followership(MongoDBRef initiator, MongoDBRef target) {
		this.Initiator = initiator;
		this.Target = target;
	}
}
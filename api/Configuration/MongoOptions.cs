namespace PartyJukebox.Api.Configuration;

public class MongoOptions
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; set; } = "mongodb://mongodb:27017";

    public string Database { get; set; } = "partyjukebox";
}

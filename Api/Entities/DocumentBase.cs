using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Entities;

public class DocumentBase
{
    [BsonId]
    public ObjectId Id { get; set; }
    
    public DateTime CreationDate { get; set; }

    public DocumentBase()
    {
        CreationDate = DateTime.UtcNow;
    }
}
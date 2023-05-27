using System.Linq.Expressions;
using Api.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Api.Repository;

public class BaseRepository<TDocument> : IBaseRepository<TDocument> where TDocument : DocumentBase
{
    private readonly IMongoCollection<TDocument> _collection;

    public BaseRepository()
    {
        var database = new MongoClient("mongodb://localhost:27017").GetDatabase("bsuir");
        _collection = database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
        FilterDefinitionBuilder = new FilterDefinitionBuilder<TDocument>();
        SortDefinitionBuilder = new SortDefinitionBuilder<TDocument>();
    }

    public FilterDefinitionBuilder<TDocument> FilterDefinitionBuilder { get; }
    public SortDefinitionBuilder<TDocument> SortDefinitionBuilder { get; }

    public virtual IQueryable<TDocument> AsQueryable()
    {
        return _collection.AsQueryable();
    }

    public virtual IEnumerable<TDocument> FilterBy(
        Expression<Func<TDocument, bool>> filterExpression)
    {
        return _collection.Find(filterExpression).ToEnumerable();
    }

    public virtual Task<IEnumerable<TDocument>> FilterByAsync(
        Expression<Func<TDocument, bool>> filterExpression)
    {
        return Task.Run(() => { return _collection.Find(filterExpression).ToEnumerable(); });
    }

    public virtual IEnumerable<TDocument> GetAll()
    {
        return _collection.Find(_ => true).ToEnumerable();
    }

    public virtual Task<IEnumerable<TDocument>> GetAllAsync()
    {
        return Task.Run(() => { return _collection.Find(_ => true).ToEnumerable(); });
    }

    public virtual IEnumerable<TProjected> FilterBy<TProjected>(
        Expression<Func<TDocument, bool>> filterExpression,
        Expression<Func<TDocument, TProjected>> projectionExpression)
    {
        return _collection.Find(filterExpression).Project(projectionExpression).ToEnumerable();
    }

    public virtual async Task<IEnumerable<TDocument>> FilterByAsync(
        IEnumerable<FilterDefinition<TDocument>> filterDefinitions, SortDefinition<TDocument> sortDefinition = null)
    {
        var findOptions = new FindOptions<TDocument>
        {
            Sort = sortDefinition
        };

        var documents = await _collection.FindAsync(FilterDefinitionBuilder.And(filterDefinitions), findOptions);
        return documents.ToEnumerable();
    }

    public virtual TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression)
    {
        return _collection.Find(filterExpression).FirstOrDefault();
    }

    public virtual async Task<TDocument?> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression,
        SortDefinition<TDocument> sortDefinition = null)
    {
        var findOptions = new FindOptions<TDocument>
        {
            Sort = sortDefinition
        };

        var documents = await _collection.FindAsync(filterExpression, findOptions);
        var document = await documents.FirstOrDefaultAsync();
        return document;
    }

    public virtual async Task<TDocument> FindOneAsync(FilterDefinition<TDocument> filterDefinition,
        SortDefinition<TDocument> sortDefinition = null)
    {
        var findOptions = new FindOptions<TDocument>
        {
            Sort = sortDefinition
        };

        var documents = await _collection.FindAsync(filterDefinition, findOptions);
        var document = await documents.FirstOrDefaultAsync();
        return document;
    }

    public virtual async Task<TDocument> FindOneAsync(IEnumerable<FilterDefinition<TDocument>> filterDefinitions,
        SortDefinition<TDocument> sortDefinition = null)
    {
        var findOptions = new FindOptions<TDocument>
        {
            Sort = sortDefinition
        };

        var documents = await _collection.FindAsync(FilterDefinitionBuilder.And(filterDefinitions), findOptions);
        var document = await documents.FirstOrDefaultAsync();
        return document;
    }

    public virtual List<FilterDefinition<TDocument>> GetFilterDefinitionsFromString(string filterString)
    {
        var filterDefinitions = new List<FilterDefinition<TDocument>> { FilterDefinitionBuilder.Empty };
        if (string.IsNullOrWhiteSpace(filterString)) return filterDefinitions;

        var filters = filterString.Split(";");
        foreach (var filter in filters)
        {
            var filterParts = filter.Split("-");
            var property = filterParts[0];
            var action = filterParts[1].ToLower();
            var value = filterParts[2];

            if (action == "eq")
            {
                var values = value.Split("||");
                var properties = property.Split("||");
                List<FilterDefinition<TDocument>> _filters = new();

                foreach (var _property in properties)
                foreach (var _value in values)
                    _filters.Add(FilterDefinitionBuilder.Eq(_property, _value));
                filterDefinitions.Add(FilterDefinitionBuilder.Or(_filters));
            }
            else if (action == "ne")
            {
                var values = value.Split("||");
                var properties = property.Split("||");
                List<FilterDefinition<TDocument>> _filters = new();

                foreach (var _property in properties)
                foreach (var _value in values)
                    _filters.Add(FilterDefinitionBuilder.Ne(_property, _value));
                filterDefinitions.Add(FilterDefinitionBuilder.Or(_filters));
            }
            else if (action == "isnull")
            {
                var values = value.Split("||");
                var properties = property.Split("||");
                List<FilterDefinition<TDocument>> _filters = new();

                foreach (var _property in properties)
                foreach (var _value in values.Select(x => bool.Parse(x)))
                    if (_value) _filters.Add(FilterDefinitionBuilder.Eq<object>(_property, null));
                    else _filters.Add(FilterDefinitionBuilder.Ne<object>(_property, null));
                filterDefinitions.Add(FilterDefinitionBuilder.Or(_filters));
            }
            else if (action == "contains")
            {
                var values = value.Split("||");
                var properties = property.Split("||");
                List<FilterDefinition<TDocument>> _filters = new();

                foreach (var _property in properties)
                foreach (var _value in values)
                {
                    var regexString = @$"\w*(?i){value}";
                    _filters.Add(FilterDefinitionBuilder.Regex(_property, regexString));
                }

                filterDefinitions.Add(FilterDefinitionBuilder.Or(_filters));
            }
            else if (action == "in")
            {
                var values = value.Split(";");
                filterDefinitions.Add(FilterDefinitionBuilder.In(property, values));
            }
            else
            {
                throw new InvalidOperationException($"Invalid action: {action}");
            }
        }

        return filterDefinitions;
    }

    public virtual TDocument FindById(ObjectId id)
    {
        //var ObjectId = new ObjectId(id);
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
        return _collection.Find(filter).SingleOrDefault();
    }

    public virtual Task<TDocument> FindByIdAsync(ObjectId id)
    {
        return Task.Run(() =>
        {
            //var ObjectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
            return _collection.Find(filter).SingleOrDefaultAsync();
        });
    }

    public virtual void InsertOne(TDocument document)
    {
        _collection.InsertOne(document);
    }

    public virtual Task InsertOneAsync(TDocument document)
    {
        return Task.Run(() => _collection.InsertOneAsync(document));
    }

    public virtual void InsertMany(ICollection<TDocument> documents)
    {
        if (documents.Count > 0) _collection.InsertMany(documents);
    }

    public virtual async Task InsertManyAsync(ICollection<TDocument> documents)
    {
        await _collection.InsertManyAsync(documents);
    }

    public virtual void ReplaceOne(TDocument document)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        _collection.FindOneAndReplace(filter, document);
    }

    public virtual async Task ReplaceOneAsync(TDocument document)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
        await _collection.FindOneAndReplaceAsync(filter, document);
    }

    public virtual async Task<long> GetCountAsync(Expression<Func<TDocument, bool>> filterExpression)
    {
        return await _collection.CountDocumentsAsync(filterExpression);
    }

    public virtual async Task<long> GetCountAsync(FilterDefinition<TDocument> filterDefinition)
    {
        return await _collection.CountDocumentsAsync(filterDefinition);
    }

    public virtual async Task<long> GetCountAsync(IEnumerable<FilterDefinition<TDocument>> filterDefinitions)
    {
        return await _collection.CountDocumentsAsync(FilterDefinitionBuilder.And(filterDefinitions));
    }

    public virtual async Task<IEnumerable<TDocument>> FilterWithSkipAsync(
        Expression<Func<TDocument, bool>> filterExpression, int skip, int count)
    {
        var findOptions = new FindOptions<TDocument>
        {
            Skip = skip,
            Limit = count
        };

        var documents = await _collection.FindAsync(filterExpression, findOptions);
        return documents.ToEnumerable();
    }

    public virtual async Task<IEnumerable<TDocument>> FilterWithSkipAsync(FilterDefinition<TDocument> filterDefinition,
        int skip, int count, SortDefinition<TDocument> sortDefinition = null)
    {
        var findOptions = new FindOptions<TDocument>
        {
            Skip = skip,
            Limit = count,
            Sort = sortDefinition
        };

        var documents = await _collection.FindAsync(filterDefinition, findOptions);
        return documents.ToEnumerable();
    }

    public virtual async Task<IEnumerable<TDocument>> FilterWithSkipAsync(
        IEnumerable<FilterDefinition<TDocument>> filterDefinitions, int skip, int count,
        SortDefinition<TDocument> sortDefinition = null)
    {
        var findOptions = new FindOptions<TDocument>
        {
            Skip = skip,
            Limit = count,
            Sort = sortDefinition
        };

        var documents = await _collection.FindAsync(FilterDefinitionBuilder.And(filterDefinitions), findOptions);
        return documents.ToEnumerable();
    }

    public virtual async Task HardDeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression)
    {
        await _collection.DeleteManyAsync(filterExpression);
    }

    public async Task HardDeleteManyAsync(IEnumerable<FilterDefinition<TDocument>> filterDefinitions)
    {
        await _collection.DeleteManyAsync(FilterDefinitionBuilder.And(filterDefinitions));
    }

    private string GetCollectionName(Type documentType)
    {
        return ((BsonCollectionAttribute)documentType.GetCustomAttributes(
                typeof(BsonCollectionAttribute),
                true)
            .FirstOrDefault())?.CollectionName;
    }
}
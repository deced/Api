using System.Linq.Expressions;
using Api.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Api.Repository;

public interface IBaseRepository<TDocument> where TDocument : DocumentBase
{
    FilterDefinitionBuilder<TDocument> FilterDefinitionBuilder { get; }

    SortDefinitionBuilder<TDocument> SortDefinitionBuilder { get; }

    IQueryable<TDocument> AsQueryable();

    IEnumerable<TDocument> FilterBy(Expression<Func<TDocument, bool>> filterExpression);

    Task<IEnumerable<TDocument>> FilterByAsync(Expression<Func<TDocument, bool>> filterExpression);

    Task<IEnumerable<TDocument>> FilterByAsync(IEnumerable<FilterDefinition<TDocument>> filterDefinitions,
        SortDefinition<TDocument> sortDefinition = null);

    IEnumerable<TDocument> GetAll();

    Task<IEnumerable<TDocument>> GetAllAsync();

    IEnumerable<TProjected> FilterBy<TProjected>(
        Expression<Func<TDocument, bool>> filterExpression,
        Expression<Func<TDocument, TProjected>> projectionExpression);

    TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression);

    Task<TDocument?> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression,
        SortDefinition<TDocument> sortDefinition = null);

    Task<TDocument> FindOneAsync(FilterDefinition<TDocument> filterDefinition,
        SortDefinition<TDocument> sortDefinition = null);

    Task<TDocument> FindOneAsync(IEnumerable<FilterDefinition<TDocument>> filterDefinitions,
        SortDefinition<TDocument> sortDefinition = null);

    /// <summary>
    ///     Creating list of filter definitions
    /// </summary>
    /// <param name="filterString">String of filters. Filter format: "Property"-"Action"-"Value";...</param>
    /// <returns>List of filter definitions</returns>
    List<FilterDefinition<TDocument>> GetFilterDefinitionsFromString(string filterString);

    TDocument FindById(ObjectId id);

    Task<TDocument> FindByIdAsync(ObjectId id);

    void InsertOne(TDocument document);

    Task InsertOneAsync(TDocument document);

    void InsertMany(ICollection<TDocument> documents);

    Task InsertManyAsync(ICollection<TDocument> documents);

    void ReplaceOne(TDocument document);

    Task ReplaceOneAsync(TDocument document);

    Task<long> GetCountAsync(Expression<Func<TDocument, bool>> filterExpression);

    Task<long> GetCountAsync(FilterDefinition<TDocument> filterDefinition);

    Task<long> GetCountAsync(IEnumerable<FilterDefinition<TDocument>> filterDefinitions);

    Task<IEnumerable<TDocument>> FilterWithSkipAsync(Expression<Func<TDocument, bool>> filterExpression, int skip,
        int count);

    Task<IEnumerable<TDocument>> FilterWithSkipAsync(FilterDefinition<TDocument> filterDefinition, int skip, int count,
        SortDefinition<TDocument> sortDefinition = null);

    Task<IEnumerable<TDocument>> FilterWithSkipAsync(IEnumerable<FilterDefinition<TDocument>> filterDefinitions,
        int skip, int count, SortDefinition<TDocument> sortDefinition = null);

    Task HardDeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression);

    Task HardDeleteManyAsync(IEnumerable<FilterDefinition<TDocument>> filterDefinitions);
}
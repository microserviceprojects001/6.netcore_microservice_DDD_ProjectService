using MongoDB.Driver;
using Contact.API.Models;
using Microsoft.Extensions.Options;

namespace Contact.API.Data;

public class ContactContext
{
    private IMongoDatabase _database;
    private readonly AppSettings _appSettings;
    private IMongoCollection<ContactBook> _contactBooks;


    public ContactContext(IOptionsMonitor<AppSettings> appSettings)
    {
        _appSettings = appSettings.CurrentValue; // 通过 CurrentValue 访问

        var client = new MongoClient(_appSettings.MongoContactConnectionString);
        if (client != null)
        {
            _database = client.GetDatabase(_appSettings.MongoContactDatabaseName);
        }

    }

    public IMongoCollection<ContactBook> ContactBooks
    {
        get
        {
            if (_contactBooks == null)
            {
                CheckAndCreateCollection("ContactBooks");
                _contactBooks = _database.GetCollection<ContactBook>("ContactBooks");
            }
            return _contactBooks;
        }
    }

    public IMongoCollection<ContactApplyRequest> ContactApplyRequests
    {
        get
        {
            CheckAndCreateCollection("ContactBooks");
            return _database.GetCollection<ContactApplyRequest>("ContactApplyRequests");
        }
    }

    private void CheckAndCreateCollection(string collectionName)
    {
        if (_database.ListCollectionNames().ToList().All(name => name != collectionName))
        {
            _database.CreateCollection(collectionName);
        }
    }
}
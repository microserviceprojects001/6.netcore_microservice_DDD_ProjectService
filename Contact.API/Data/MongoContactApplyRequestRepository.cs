using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Contact.API.Models;

namespace Contact.API.Data;

public class MongoContactApplyRequestRepository : IContactApplyRequestRepository
{
    private readonly ContactContext _contactContext;
    public MongoContactApplyRequestRepository(ContactContext contactContext)
    {
        Console.WriteLine($"[{DateTime.Now}] MongoContactApplyRequestRepository 构造函数执行");
        _contactContext = contactContext;

    }

    public async Task<bool> AddRequestAsync(ContactApplyRequest request, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ContactApplyRequest>.Filter.Where(r => r.UserId == request.UserId && r.ApplierId == request.ApplierId);

        if ((await _contactContext.ContactApplyRequests.CountAsync(filter, null, cancellationToken)) > 0)
        {
            var update = Builders<ContactApplyRequest>.Update
                .Set(r => r.ApplyTime, DateTime.Now);

            //var options = new UpdateOptions { IsUpsert = true };
            var result = await _contactContext.ContactApplyRequests.UpdateOneAsync(filter, update, null, cancellationToken);
            return result.MatchedCount == 1 && result.MatchedCount == result.ModifiedCount;
        }
        await _contactContext.ContactApplyRequests.InsertOneAsync(request, null, cancellationToken);
        return true;
    }

    public async Task<bool> ApprovalAsync(int userId, int applierId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ContactApplyRequest>.Filter.Where(r => r.UserId == userId && r.ApplierId == applierId);

        var update = Builders<ContactApplyRequest>.Update
            .Set(r => r.Approvaled, 1)
            .Set(r => r.HandledTime, DateTime.Now);

        //var options = new UpdateOptions { IsUpsert = true };
        var result = await _contactContext.ContactApplyRequests.UpdateOneAsync(filter, update, null, cancellationToken);
        return result.MatchedCount == 1 && result.MatchedCount == result.ModifiedCount;

    }

    public async Task<List<ContactApplyRequest>> GetRequestListAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _contactContext.ContactApplyRequests.Find(r => r.UserId == userId).ToListAsync(cancellationToken);
    }
}

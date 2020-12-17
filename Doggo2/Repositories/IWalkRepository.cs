using Doggo2.Models;
using System.Collections.Generic;

namespace Doggo2.Repositories
{
    public interface IWalkRepository
    {
        List<Walk> GetAllWalks();
        Walk GetWalkById(int id);

        List<Walk> GetWalkByWalkerId(int walkerId);

    }
}
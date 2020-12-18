using Doggo2.Models;
using System.Collections.Generic;

namespace Doggo2.Repositories
{
    public interface IWalkerRepository
    {
        List<Walker> GetAllWalkers();
        Walker GetWalkerById(int id);
        List<Walker> GetWalkersInNeighborhood(int neighborhoodId);

        void AddWalker(Walker walker);
        void UpdateWalker(Walker walker);
        void DeleteWalker(int walkerId);
    }
}
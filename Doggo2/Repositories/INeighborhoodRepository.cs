using Doggo2.Models;
using System.Collections.Generic;

namespace Doggo2.Repositories
{
    public interface INeighborhoodRepository
    {
        List<Neighborhood> GetAll();
    }
}
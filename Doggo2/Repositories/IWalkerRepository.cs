using Doggo2.Models;
using System.Collections.Generic;

namespace Doggo2.Repositories
{
    public interface IWalkerRepository
    {
        List<Walker> GetAllWalkers();
        Walker GetWalkerById(int id);
    }
}
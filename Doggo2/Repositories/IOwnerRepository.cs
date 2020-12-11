using Doggo2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Doggo2.Repositories
{
    public interface IOwnerRepository
    {
        List<Owner> GetAllOwners();
        Owner GetOwnerById(int id);
        void UpdateOwner(Owner owner);

        void DeleteOwner(int ownerId);
        void AddOwner(Owner owner);

        Owner GetOwnerByEmail();
    }
}

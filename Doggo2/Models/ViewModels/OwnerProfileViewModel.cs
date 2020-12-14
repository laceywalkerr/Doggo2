using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doggo2.Models.ViewModels
{
    public class OwnerProfileViewModel
    {
        public Owner Owner { get; set; }
        public List<Walker> WalkersInNeighborhood { get; set; }

        public List<Dog> Dog { get; set; }
    }
}

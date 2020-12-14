using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doggo2.Models.ViewModels
{
    public class OwnerFormViewModel
    {
        public Owner Owner { get; set; }
        public List<Neighborhood> NeighborhoodOptions { get; set; }

        public string ErrorMessage { get; set; }
    }
}

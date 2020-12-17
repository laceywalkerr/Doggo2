using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doggo2.Models.ViewModels
{
    public class WalkerProfileViewModel
    {
        public Walker Walker { get; set; }
        public List<Walker> Walkers { get; set; }

        public Walk Walk { get; set; } 
        
        public List<Walk> Walks { get; set; }

        public string TotalTimeWalkedDisplay { get; set; }
        public Dog Dog { get; set; }
        public List<Dog> Dogs { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Doggo2.Models
{
    public class Owner
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="You gotta have a name, dude.")]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 10)]
        public string Address { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public int NeighborhoodId { get; set; }
        [Required]
        public Neighborhood Neighborhood { get; set; }
    }
}

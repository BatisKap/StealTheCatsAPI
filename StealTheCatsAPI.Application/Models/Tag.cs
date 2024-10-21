using System;

namespace StealTheCatsAPI.Application.Models
{
    public class Tag
    {
        public int Id { get; set; } 
        public string Name { get; set; } 
        public DateTime Created { get; set; } 

        // Navigation property for Cat
        public ICollection<Cat> Cats { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace StealTheCatsAPI.Application.Models
{
    public class Cat
    {
        public int Id { get; set; } 
        public string CatId { get; set; } 
        public int Width { get; set; } 
        public int Height { get; set; } 
        public string Image { get; set; } 
        public DateTime Created { get; set; } 

        // Navigation property for Tags
        public ICollection<Tag> Tags { get; set; }
    }
}

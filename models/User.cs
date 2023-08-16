using System.ComponentModel.DataAnnotations;

namespace PetDeskDataModels {

    public class User {
        [Key]
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? VetDataId { get; set; } 
    }

}
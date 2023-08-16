using System.ComponentModel.DataAnnotations;

namespace PetDeskDataModels
{

    public class AppointmentChangeRequest {

        public AppointmentChangeRequest() {
            User = new User();
            Animal = new Animal();
        }
        [Key]
        public int AppointmentId { get; set; }
        
        // ideally Appointment Type and Status would be stored in the DB and would be an object 
        // with an Id and Description 
        public string? AppointmentType { get; set; }
        public string? Status { get; set; }

        public DateTime CreateDateTime { get; set; }
        
        public DateTime RequestedDateTimeOffset { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int AnimalId { get; set; }
        public Animal Animal { get; set; }
    }

}
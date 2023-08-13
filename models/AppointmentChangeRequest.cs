namespace PetDeskDataModels {

    public class AppointmentChangeRequest {

        public AppointmentChangeRequest() {
            User = new User();
            Animal = new Pet();
        }

        public int AppointmentId { get; set; }
        
        // ideally Appointment Type would be stored in the DB and this property would be an object 
        // with an Id and Description 
        public string? AppointmentType { get; set; }
        
        public DateTime CreateDateTime { get; set; }
        
        public DateTime RequestedDateTimeOffset { get; set; }
        
        public User User { get; set; }
        
        public Pet Animal { get; set; }
    }

}
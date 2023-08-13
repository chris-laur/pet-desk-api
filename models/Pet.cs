namespace PetDeskDataModels {
    public class Pet {
        public int AnimalId { get; set; }
        
        public string? FirstName { get; set; }
        
        // ideally Breed and Species would be stored in the DB and these properties would be objects 
        // with an Id and Description 
        public string? Species { get; set; }
        public string? Breed { get; set; }
    }

}
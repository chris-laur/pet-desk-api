using Microsoft.EntityFrameworkCore;
using PetDeskDataModels;

class PetDeskDb : DbContext {
    public PetDeskDb(DbContextOptions<PetDeskDb> options) : base(options) { }
    public DbSet<AppointmentChangeRequest> AppointmentChangeRequests => Set<AppointmentChangeRequest>();
}
using Microsoft.EntityFrameworkCore;
using PetDeskDataModels;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PetDeskDb>(opt => opt.UseInMemoryDatabase("AppointmentChangeRequests"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "PetDeskClientApp",
                      policy  =>
                      {
                          policy.WithOrigins("https://chris-laur.github.io", "http://localhost:5173");
                      });
});

var app = builder.Build();
var requests = app.MapGroup("/api");
requests.MapGet("/appointmentchangerequests", GetAppointmentChangeRequests);
requests.MapPut("/updateappointmentchangerequest/{id}", UpdateAppointmentChangeRequest);
app.UseCors("PetDeskClientApp");
app.Run();

static async Task<IResult> GetAppointmentChangeRequests(PetDeskDb db)
{
    if (!db.AppointmentChangeRequests.Any()) {

        var appointmentChangeRequests = new List<AppointmentChangeRequest>();

        try{

            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("https://723fac0a-1bff-4a20-bdaa-c625eae11567.mock.pstmn.io/appointments"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    appointmentChangeRequests = JsonSerializer.Deserialize<List<AppointmentChangeRequest>>(apiResponse, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                    db.AppointmentChangeRequests.AddRange(appointmentChangeRequests);
                    db.SaveChanges();
                }
            }
        }
        catch {
            return TypedResults.BadRequest("There was an issue getting the data.");
        }

    }

    return TypedResults.Ok(await db.AppointmentChangeRequests.ToArrayAsync());
}

static async Task<IResult> UpdateAppointmentChangeRequest(int id, AppointmentChangeRequest updatedAppointmentChangeRequest, PetDeskDb db)
{
    var appointmentChangeRequest = await db.AppointmentChangeRequests.FindAsync(id);

    if (appointmentChangeRequest is null) return TypedResults.NotFound();

    appointmentChangeRequest.RequestedDateTimeOffset = updatedAppointmentChangeRequest.RequestedDateTimeOffset;
    appointmentChangeRequest.Status = updatedAppointmentChangeRequest.Status;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}
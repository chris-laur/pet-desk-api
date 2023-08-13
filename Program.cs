using Microsoft.EntityFrameworkCore;
using PetDeskDataModels;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddDbContext<PetDeskDb>(opt => opt.UseInMemoryDatabase("AppointmentChangeRequests"));
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

var requests = app.MapGroup("/appointmentchangerequests");
requests.MapGet("/", GetAppointmentChangeRequests);

app.Run();

async Task<IResult> GetAppointmentChangeRequests(/*PetDeskDb db*/)
{
    var requests = new List<AppointmentChangeRequest>();
    
    using (var httpClient = new HttpClient())
    {
        using (var response = await httpClient.GetAsync("https://723fac0a-1bff-4a20-bdaa-c625eae11567.mock.pstmn.io/appointments"))
        {
            string apiResponse = await response.Content.ReadAsStringAsync();
            requests = JsonSerializer.Deserialize<List<AppointmentChangeRequest>>(apiResponse, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }
    }

    return TypedResults.Ok(requests);
}
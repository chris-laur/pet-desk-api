using Microsoft.AspNetCore.Cors;
//using Microsoft.EntityFrameworkCore;
using PetDeskDataModels;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddDbContext<PetDeskDb>(opt => opt.UseInMemoryDatabase("AppointmentChangeRequests"));
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "PetDeskClientApp",
                      policy  =>
                      {
                          policy.WithOrigins("https://chris-laur.github.io").WithMethods("GET");
                      });
});

var app = builder.Build();

var requests = app.MapGroup("/api");

requests.MapGet("/appointmentchangerequests", GetAppointmentChangeRequests);

app.UseCors("PetDeskClientApp");
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
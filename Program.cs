using Microsoft.EntityFrameworkCore;
using PetDeskDataModels;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// DbContext (InMemory for local/dev). Replace with a secure provider for production.
builder.Services.AddDbContext<PetDeskDb>(opt => opt.UseInMemoryDatabase("AppointmentChangeRequests"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Register HttpClientFactory to avoid socket exhaustion and centralize configuration.
builder.Services.AddHttpClient("petdesk");

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "PetDeskClientApp",
                      policy  =>
                      {
                          policy.WithOrigins("https://chris-laur.github.io", "http://localhost:5173").AllowAnyHeader().AllowAnyMethod();
                      });
});

var app = builder.Build();

// Enforce HTTPS and HSTS in non-development environments
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

// If an API key is configured, require it on incoming requests.
var configuredApiKey = app.Configuration["ApiKey"];
if (!string.IsNullOrEmpty(configuredApiKey))
{
    app.Use(async (context, next) =>
    {
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var extractedApiKey) || extractedApiKey != configuredApiKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }
        await next();
    });
}

app.UseCors("PetDeskClientApp");

var requests = app.MapGroup("/api");
requests.MapGet("/appointmentchangerequests", GetAppointmentChangeRequests);
requests.MapPut("/updateappointmentchangerequest/{id}", UpdateAppointmentChangeRequest);

app.Run();

static async Task<IResult> GetAppointmentChangeRequests(PetDeskDb db, IHttpClientFactory httpFactory, IConfiguration config)
{
    if (!db.AppointmentChangeRequests.Any())
    {
        var appointmentChangeRequests = new List<AppointmentChangeRequest>();
        var appointmentsUrl = config["ExternalApis:AppointmentsUrl"];
        if (string.IsNullOrEmpty(appointmentsUrl))
        {
            return TypedResults.Problem("External API URL not configured", statusCode: 500);
        }

        try
        {
            var client = httpFactory.CreateClient("petdesk");
            using var response = await client.GetAsync(appointmentsUrl);
            response.EnsureSuccessStatusCode();
            var apiResponse = await response.Content.ReadAsStringAsync();
            appointmentChangeRequests = JsonSerializer.Deserialize<List<AppointmentChangeRequest>>(apiResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AppointmentChangeRequest>();

            await db.AppointmentChangeRequests.AddRangeAsync(appointmentChangeRequests);
            await db.SaveChangesAsync();
        }
        catch (HttpRequestException)
        {
            return TypedResults.Problem("Error calling external API", statusCode: 502);
        }
        catch (JsonException)
        {
            return TypedResults.Problem("Invalid data from external API", statusCode: 502);
        }
        catch (Exception)
        {
            return TypedResults.Problem("There was an issue getting the data.", statusCode: 500);
        }
    }

    return TypedResults.Ok(await db.AppointmentChangeRequests.Include(x => x.Animal).Include(x => x.User).ToArrayAsync());
}

static readonly string[] AllowedStatuses = new[] { "Pending", "Approved", "Denied" };

static async Task<IResult> UpdateAppointmentChangeRequest(int id, AppointmentChangeRequest updatedAppointmentChangeRequest, PetDeskDb db)
{
    if (updatedAppointmentChangeRequest is null)
    {
        return TypedResults.BadRequest("Invalid payload");
    }

    if (!AllowedStatuses.Contains(updatedAppointmentChangeRequest.Status))
    {
        return TypedResults.BadRequest("Invalid status value");
    }

    if (updatedAppointmentChangeRequest.RequestedDateTimeOffset < DateTimeOffset.UtcNow.AddMinutes(-5))
    {
        return TypedResults.BadRequest("Requested date/time is in the past");
    }

    var appointmentChangeRequest = await db.AppointmentChangeRequests.FindAsync(id);

    if (appointmentChangeRequest is null) return TypedResults.NotFound();

    appointmentChangeRequest.RequestedDateTimeOffset = updatedAppointmentChangeRequest.RequestedDateTimeOffset;
    appointmentChangeRequest.Status = updatedAppointmentChangeRequest.Status;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}
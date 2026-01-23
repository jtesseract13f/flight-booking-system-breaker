using DAL;
using FlightServiceApi;
using FlightServiceApi.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<FlightDbContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddSwaggerGen();

var app = builder.Build();

try //Migrator
{
    using var scope = ((IApplicationBuilder)app).ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
    scope.ServiceProvider.GetRequiredService<FlightDbContext>().Database.Migrate();
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}
await app.Seed();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.MapGet("/manage/health", () => StatusCodes.Status200OK);
//TO DO: test all
app.MapGet("/flights/{flightNumber}", (string flightNumber, FlightDbContext context) =>
{
    var entity = context.Flights
        .Include(x => x.FromAirport)
        .Include(x=>x.ToAirport)
        .FirstOrDefault(x => x.FlightNumber == flightNumber);
    if (entity == null) return null;
    return new FlightDto(
        entity.FlightNumber,
        entity.Datetime,
        entity.FromAirport.City + " " + entity.FromAirport.Name,
        entity.ToAirport.City+ " " + entity.ToAirport.Name,
        entity.Price);
});

app.MapGet("/flights", ([FromQuery] int page, [FromQuery] int size, FlightDbContext context) =>
    {
        var flightEntities = context.Flights
            .Include(x=>x.FromAirport)
            .Include(x=>x.ToAirport)
            .Skip((page-1) * size).Take(size);
        var flights = flightEntities
            .Select(x => new FlightDto(x.FlightNumber,  x.Datetime, x.FromAirport.City + " " + x.FromAirport.Name, 
                x.ToAirport.City + " " +x.ToAirport.Name, x.Price))
            .ToList();
        return flights;
    }
);

app.Run();

using DAL;
using Microsoft.EntityFrameworkCore;

namespace FlightServiceApi;

public static class DbExtensions
{
    public static async Task Seed(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FlightDbContext>();
            //seed airports
            if (!await context.Airports.AnyAsync())
            {
                var airports = new[]
                {
                    new Airport
                    {
                        Name = "Шереметьево",
                        City = "Москва",
                        Country = "Россия"
                    },
                    new Airport
                    {
                        Name = "Пулково",
                        City = "Санкт-Петербург",
                        Country = "Россия"
                    }
                };
                await context.Airports.AddRangeAsync(airports);
                await context.SaveChangesAsync();
            }
            //seed flights
            if (!await context.Flights.AnyAsync())
            {
                var airports = await context.Airports.ToListAsync();
                var flights = new[]
                {
                    new Flight()
                    {
                        FlightNumber = "AFL031",
                        Datetime = DateTime.Parse("2021-10-08 20:00").ToUniversalTime(),
                        ToAirport= airports.First(x =>  x.City == "Москва"),
                        ToAirportId = airports.First(x =>  x.City == "Москва").Id,
                        FromAirport = airports.First(x  =>  x.City == "Санкт-Петербург"),
                        FromAirportId= airports.First(x =>  x.City == "Санкт-Петербург").Id,
                        Price = 1500
                    }
                };
        
                await context.Flights.AddRangeAsync(flights);
                await context.SaveChangesAsync();
            }
            Console.WriteLine("Database seeded with initial data");
        }
    }
}
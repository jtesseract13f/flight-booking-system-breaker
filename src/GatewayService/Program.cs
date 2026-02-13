using System.ComponentModel.DataAnnotations;
using GatewayService.ApiServices;
using GatewayService.BLL;
using GatewayService.DTO;
using GatewayService.DTO.FlightApiDtos;
using GatewayService.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.CircuitBreaker;
using Refit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<PolicyAwareHandler>();
builder.Services.AddTransient<CircuitBreaker>();
var servicesConfig = builder.Configuration.GetSection("Microservices");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRefitClient<IFlightApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(servicesConfig["FlightServiceApi"] ?? throw new InvalidOperationException()))
    .AddHttpMessageHandler<CircuitBreaker>()
    .AddHttpMessageHandler<PolicyAwareHandler>();
builder.Services.AddRefitClient<ITicketApi>()
    .ConfigureHttpClient(c =>
        c.BaseAddress = new Uri(servicesConfig["TicketServiceApi"] ?? throw new InvalidOperationException()))
    .AddHttpMessageHandler<CircuitBreaker>()
    .AddHttpMessageHandler<PolicyAwareHandler>();
builder.Services.AddRefitClient<IBonusApi>()
    .ConfigureHttpClient(c =>
        c.BaseAddress = new Uri(servicesConfig["BonusServiceApi"] ?? throw new InvalidOperationException()))
    .AddHttpMessageHandler<CircuitBreaker>()
    .AddHttpMessageHandler<PolicyAwareHandler>();

builder.Services.AddRefitClient<IMockApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://flight-service:9091/"))
    .AddHttpMessageHandler<CircuitBreaker>()
    .AddHttpMessageHandler<PolicyAwareHandler>();

builder.Services.AddHttpClient("configured-inner-client")
    .AddHttpMessageHandler<CircuitBreaker>()
    .AddHttpMessageHandler<PolicyAwareHandler>();

builder.Services.AddScoped<BookingService>();

builder.Services.AddHostedService<QueueWorkerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        context.Response.ContentType = "application/json";

        context.Response.StatusCode = error switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new
        {
            error = error?.Message,
            trace =  error?.StackTrace,
            code = context.Response.StatusCode
        };

        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.MapGet("/manage/health", () => StatusCodes.Status200OK);

var apiV1 = app.MapGroup("/api/v1");

apiV1.MapGet("/flights", async ([FromQuery] int page, [FromQuery] int size, IFlightApi api) =>
        {
            var result = await api.GetAllFlightInfos(page, size);
            if (result is null) return null;
            return new PaginationList<Flight>()
            {
                Page = page,
                PageSize = size,
                Items = result,
                TotalElements = result.Count()
            };
        }
  )
    .WithDescription("Получить список рейсов")
    .WithOpenApi();

apiV1.MapGet("/check-fallback", async ([FromHeader(Name = "X-User-Name")]string username,
        IMockApi api) =>
    {
        try
        {
            var res = await api.GetFlightInfo("");
            return "Оно не должно работать.";
        }
        catch (BrokenCircuitException e)
        {
            return "синий синий сеньор упал на провода, сервер не поднимется больше никогда: " + e.Message ;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "Успех! Ничего не работает: " + e.Message;
        }
    })
    .WithDescription("Тестирование fallback")
    .WithOpenApi();


//TODO: Test
apiV1.MapGet("/privilege", async ([FromHeader(Name = "X-User-Name")]string username,
        IBonusApi api) => await api.GetBalanceInfo(username))
    .WithDescription("Получить информацию о состоянии бонусного счета")
    .WithOpenApi();

//TODO: Test; ADD BONUS API
apiV1.MapPost("/tickets", async ([FromHeader(Name = "X-User-Name")]string username, [FromBody] BuyTicket ticket, BookingService service) => 
    await service.BuyTicket(username, ticket))
    .WithDescription("Покупка билета")
    .WithOpenApi();
//TODO: Test
apiV1.MapGet("/tickets/{ticketUid}", async ([FromHeader(Name = "X-User-Name")]string username, [FromRoute] Guid ticketUid, BookingService service) => 
    await service.GetTicketInfo(username, ticketUid))
    .WithDescription("Информация по конкретному билету")
    .WithOpenApi();

//TODO: Test
apiV1.MapGet("/tickets", async ([FromHeader(Name = "X-User-Name")] string username, BookingService service) =>
    {
        return await service.GetUserTickets(username);
    })
    .WithDescription("Информация по всем билетам пользователя")
    .WithOpenApi();

//!!!
apiV1.MapGet("/me", async ([FromHeader(Name = "X-User-Name")]string username,
        BookingService service) => await service.GetUser(username))
    .WithDescription("Информация о пользователе")
    .WithOpenApi();

//TODO: Test
apiV1.MapDelete("/tickets/{ticketUid}",
    async ([FromHeader(Name = "X-User-Name")] string username, [FromRoute] Guid ticketUid, BookingService service) =>
    {
        await service.CancelTicket(username,ticketUid);
        return Results.NoContent();
    })
    .WithDescription("Возврат билета")
    .WithOpenApi();

app.Run();
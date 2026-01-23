using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketServiceApi;
using TicketServiceApi.DAL;
using TicketServiceApi.DTO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TicketDbContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
var app = builder.Build();

try //Migrator
{
    using var scope = ((IApplicationBuilder)app).ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
    scope.ServiceProvider.GetRequiredService<TicketDbContext>().Database.Migrate();
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.MapGet("/manage/health", () => StatusCodes.Status200OK);
var apiV1 = app.MapGroup("/api/v1");

apiV1.MapGet("/tickets", ([FromHeader(Name = "X-User-Name")] string username,
        TicketDbContext context) =>
    {
        var tickets = context.Tickets.Where(x => x.Username == username)
            .ToList();
        var ticketDtos = tickets.Select(x => new TicketDto(
            x.TicketUid,
            x.Username,
            x.FlightNumber,
            x.Price,
            Enum.GetName(typeof(TicketStatusEnum), x.Status)));
        return ticketDtos;
    }
    );

//TODO: Test 
apiV1.MapGet("/tickets/{ticketUid}", ([FromHeader(Name = "X-User-Name")] string username,
    Guid ticketUid,
    TicketDbContext context) =>
    {
        var entity = context.Tickets.FirstOrDefault(t => t.TicketUid == ticketUid);
        if (entity == null) return null;
        //if (username != entity.Username) return null;
        var statusString = Enum.GetName(typeof(TicketStatusEnum), entity.Status);
        return new TicketInfo(entity.TicketUid,
            entity.FlightNumber,
            entity.Username,
            entity.Price, 
            statusString);
    }
);//Enum.GetName(typeof(EnumType),instanceOfEnum)

//TODO: Test
apiV1.MapDelete("/tickets/{ticketUid}", (Guid ticketUid, TicketDbContext context) =>
{
    var entity = context.Tickets.FirstOrDefault(t => t.TicketUid == ticketUid);
    if (entity == null) return Guid.Empty;
    entity.Status = TicketStatusEnum.CANCELED;
    context.Tickets.Update(entity);
    context.SaveChanges();
    return entity.TicketUid;
});

//TODO: create ticket
apiV1.MapPost("/tickets", (
    [FromBody] FlightDto flight,
    [FromQuery] int price,
    [FromHeader(Name = "X-User-Name")] string username,
    TicketDbContext context) =>
{
    var entity = new Ticket()
    {
        FlightNumber = flight.FlightNumber,
        Price = price,
        Status = TicketStatusEnum.PAID ,
        TicketUid = Guid.NewGuid(),
        Username = username,
    };
    context.Tickets.Add(entity);
    context.SaveChanges();
    var statusString = Enum.GetName(typeof(TicketStatusEnum), TicketStatusEnum.PAID);
    return new CreatedTicket(
        entity.TicketUid,
        username,
        flight.FlightNumber,
        price,
        statusString);
});
/*
 * public record Ticket(
   Guid TicketUid,
   string Username,
   string FlightNumber,
   int Price,
   string Status);//??

 */

app.Run();
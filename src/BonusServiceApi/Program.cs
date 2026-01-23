using BonusServiceApi.BLL;
using BonusServiceApi.DAL;
using BonusServiceApi.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<BonusDbContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<BonusService>();
builder.Services.AddSwaggerGen();

var app = builder.Build();
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");

try //Migrator
{
    using var scope = ((IApplicationBuilder)app).ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope();
    scope.ServiceProvider.GetRequiredService<BonusDbContext>().Database.Migrate();
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

app.MapGet("/manage/health", () => StatusCodes.Status200OK);

//TODO: Test
app.MapGet("/privilege", async ([FromHeader(Name = "X-User-Name")] string username,
    BonusService service
    ) => await service.GetPrivilege(username));
//TODO: Test
app.MapGet("/privilege/purchase", async ([FromHeader(Name = "X-User-Name")] string username,
    int price, 
    bool useBalance, 
    BonusService service) => await service.GetPurchaseInfo(username, price, useBalance));
//TODO: Test
app.MapPost("/privilege/purchase",  async ([FromHeader(Name = "X-User-Name")] string username,
    [FromBody] TicketPurchase purchase,
    BonusService service) => await service.ChangeBalance(username, purchase));
//TODO: Test
app.MapDelete("/privilege/purchase", async([FromHeader(Name = "X-User-Name")] string username,
    Guid ticketGuid,
    BonusService service) => await service.RevertPurchase(username, ticketGuid));

app.Run();

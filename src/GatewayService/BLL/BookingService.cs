using GatewayService.ApiServices;
using GatewayService.DTO;
using GatewayService.DTO.BonusServiceDtos;
using GatewayService.DTO.FlightApiDtos;
using GatewayService.DTO.TicketServiceDtos;
using Polly.CircuitBreaker;

namespace GatewayService.BLL;

public class BookingService(IBonusApi bonusService, IFlightApi flightService, ITicketApi ticketService)
{
    /*
     * Покупка билета
       Запрос к Flight Service для проверки, что такой рейс существует. 
       Если Flight Service недоступен, то запрос завершается с ошибкой.
       Выполняется запрос к Ticket Service на создание записи о билете. 
       Если сервис недоступен, то запрос завершается с ошибкой.
       Если при покупке билета указан флаг paidFromBalance, то в Bonus Service выполняется запрос на списание бонусов.
        Иначе, выполнится запрос на пополнение бонусного счета на 10% от стоимости заказа. 
        В любом случае в Bonus Service будет создана запись в таблице privilege_history.
       Если запрос к Bonus Service завершился неудачей (500 ошибка или сервис недоступен), то выполняется откат операции создания заказа в Ticket Service.
     */
    public async Task<PurchasedTicketInfo> BuyTicket(string username, BuyTicket request)
    {
        Flight flight;
        try
        {
            flight = await flightService.GetFlightInfo(request.FlightNumber);
        }
        catch (Exception e)
        {
            throw;
        }
        if (flight == null) throw new NotFoundException($"Flight {request.FlightNumber} not found");
        if (flight.Price != request.Price) throw new BadHttpRequestException("Incorrect price");
        
        PurchaseInfo purchaseInfo;
        try
        {
            purchaseInfo = await bonusService.GetPurchaseInfo(username, request.Price, request.PaidFromBalance);
        }
        catch (BrokenCircuitException)
        {
            Console.WriteLine("FLIGHT CIRCUIT FAILED");
            throw;
        }

        CreatedTicket createdTicket;
        try
        {
            createdTicket = await ticketService.CreateTicket(flight, purchaseInfo.Price, username);
        }
        catch (BrokenCircuitException)
        {
            Console.WriteLine("FLIGHT CIRCUIT FAILED");
            throw;
        }
        
        Privilege privilege;
        try
        {
            privilege = await bonusService.ChangeBalance(username,
                new TicketPurchase(createdTicket, purchaseInfo.Price, request.PaidFromBalance));
        }
        catch (BrokenCircuitException)
        {
            Console.WriteLine("FLIGHT CIRCUIT FAILED");
            await ticketService.CancelTicket(createdTicket.TicketUid);
            throw;
        }
        
        var purchasedTicketInfo = new PurchasedTicketInfo(
            createdTicket.TicketUid,
            createdTicket.FlightNumber,
            flight.FromAirport,
            flight.ToAirport,
            flight.Date,
            purchaseInfo.Price,
            purchaseInfo.PaidByMoney,
            purchaseInfo.PaidByBonuses,
            createdTicket.Status,
            privilege
            );

        return purchasedTicketInfo;
    }

    public async Task CancelTicket(string username, Guid ticketUid)
    {
        try
        {
            var result = await bonusService.RevertPurchase(username, ticketUid);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        try
        {
            var result2 = await ticketService.CancelTicket(ticketUid);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task<UserInfo?> GetUser(string username)
    {
        var tickets = new List<TicketInfo>();
        BalanceInfo privilege = null;
        try
        {
            privilege = await bonusService.GetBalanceInfo(username);
            if (privilege == null) throw new NotFoundException($"User {username} not found");
            tickets =  (await GetUserTickets(username)).ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        if (privilege == null) return new UserInfo(tickets,  new Privilege(null, null));
        return new UserInfo(tickets,  new Privilege(privilege.Balance, privilege.Status));
    }
    
    /*
     * Для методов GET /api/v1/tickets и GET /api/v1/tickets/{{ticketUid}}
     * в случае недоступности Ticket Service запрос должен вернуть 500 ошибку,
     * а в случае недоступности Flight Service, поля fromAirport, toAirport, date возвращаются как fallback значения.
     */
    public async Task<TicketInfo> GetTicketInfo(string username, Guid ticketUid)
    {
        var ticket = await ticketService.GetTicket(ticketUid, username); 
        if (ticket == null) throw new NotFoundException($"Ticket {ticketUid} not found");
        var flight = new Flight("1", null, null, null, 300);
        try
        {
            flight = await flightService.GetFlightInfo(ticket.FlightNumber);
            if (flight == null) throw new NotFoundException($"Flight {ticket.FlightNumber} not found");
        }
        catch (BrokenCircuitException)
        {
            Console.WriteLine("FLIGHT CIRCUIT FAILED");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
        }

        return new TicketInfo(
            ticket.TicketUid,
            ticket.FlightNumber,
            flight.FromAirport,
            flight.ToAirport,
            flight.Date,
            ticket.Price,
            ticket.Status);
    }
    
    /*
     * Для методов GET /api/v1/tickets и GET /api/v1/tickets/{{ticketUid}}
     * в случае недоступности Ticket Service запрос должен вернуть 500 ошибку,
     * а в случае недоступности Flight Service, поля fromAirport, toAirport, date возвращаются как fallback значения.
     */
    public async Task<IEnumerable<TicketInfo>> GetUserTickets(string username)
    {
        //crit
        var ticketsRaw = await ticketService.GetUserTickets(username);
        var flights = new Dictionary<string, Flight>();
        try
        {
            flights = (await flightService.GetAllFlightInfos(1, 100) ?? Array.Empty<Flight>()).ToDictionary(x => x.FlightNumber, x => x);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        if (flights.Count == 0)
        {
            return ticketsRaw.Select(x => new TicketInfo(
                x.TicketUid,
                x.FlightNumber,
                null,
                null,
                null,
                x.Price,
                x.Status));
        }
        return ticketsRaw.Select(x => new TicketInfo(
            x.TicketUid,
            x.FlightNumber,
            flights[x.FlightNumber]?.FromAirport,
            flights[x.FlightNumber]?.ToAirport,
            flights[x.FlightNumber]?.Date,
            x.Price,
            x.Status));
    }
}
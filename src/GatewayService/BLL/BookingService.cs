using GatewayService.ApiServices;
using GatewayService.DTO;
using GatewayService.DTO.BonusServiceDtos;
using GatewayService.DTO.FlightApiDtos;
using Polly.CircuitBreaker;

namespace GatewayService.BLL;

public class BookingService(IBonusApi bonusService, IFlightApi flightService, ITicketApi ticketService)
{
    /*
     * Пользователь вызывает метод GET {{baseUrl}}/api/v1/flights выбирает нужный рейс и в запросе на покупку передает:
       
       flightNumber (номер рейса) – берется из запроса /flights;
       price (цена) – берется из запроса /flights;
       paidFromBalance (оплата бонусами) – флаг, указывающий, что для оплаты билета нужно использовать бонусный счет.
       Система проверяет, что рейс с таким номером существует. Считаем что на рейсе бесконечное количество мест.
       
       Если при покупке указан флаг "paidFromBalance": true, то с бонусного счёта списываются максимальное количество баллов в отношении 1 балл – 1 рубль.
       Т.е. если на бонусном счете было 500 бонусов, билет стоит 1500 рублей и при покупке был указан флаг "paidFromBalance": true", то со счёта спишется 500 бонусов (в ответе будет указано "paidByBonuses": 500), а стоимость билета будет 1000 рублей (в ответе будет указано "paidByMoney": 1000). В сервисе Bonus Service в таблицу privilegeHistory будет добавлена запись о списании со счёта 500 бонусов.
       Если при покупке был указан флаг "paidFromBalance": false, то в ответе будет "paidByBonuses": 0, а на бонусный счет будет начислено бонусов в размере 10% от стоимости заказа. Так же в таблицу privilegeHistory будет добавлена запись о зачислении бонусов.
     */

    public async Task<PurchasedTicketInfo> BuyTicket(string username, BuyTicket request)
    {
        var flight = await flightService.GetFlightInfo(request.FlightNumber);
        if (flight == null) throw new NotFoundException($"Flight {request.FlightNumber} not found");
        if (flight.Price != request.Price) throw new BadHttpRequestException("Incorrect price");

        var purchaseInfo = await bonusService.GetPurchaseInfo(username, request.Price, request.PaidFromBalance);
        var createdTicket = await ticketService.CreateTicket(flight, purchaseInfo.Price, username);
        var privilege = await bonusService.ChangeBalance(username, new TicketPurchase(createdTicket, purchaseInfo.Price, request.PaidFromBalance));
        
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
    /*
     * Возврат билета
       Билет помечается статусом CANCELED, 
       в Bonus Service в зависимости 
       от типа операции выполняется возврат бонусов 
       на счёт или списание ранее начисленных. 
       При списании бонусный счет не может стать меньше 0.
       
       DELETE {{baseUrl}}/api/v1/tickets/{{ticketUid}}
       X-User-Name: {{username}}
     */

    public async Task CancelTicket(string username, Guid ticketUid)
    {
        var result = await bonusService.RevertPurchase(username, ticketUid); //TO DO: add endpoint to bonus service
        var result2 = await ticketService.CancelTicket(ticketUid);
        //TODO: revert transaction
        
        if (result != ticketUid || result2 != ticketUid) throw new Exception($"Can't cancel purchase for ticket {ticketUid}");
    }

    public async Task<UserInfo?> GetUser(string username)
    {
        var privilege = await bonusService.GetBalanceInfo(username);
        if (privilege == null) throw new NotFoundException($"User {username} not found");
        var tickets =  await GetUserTickets(username);
        return new UserInfo(tickets,  new Privilege(privilege.Balance, privilege.Status));
    }

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
     *  {
        "ticketUid": "049161bb-badd-4fa8-9d90-87c9a82b0668",
        "flightNumber": "AFL031",
        "fromAirport": "Санкт-Петербург Пулково",
        "toAirport": "Москва Шереметьево",
        "date": "2021-10-08 20:00",
        "price": 1500,
        "status": "PAID"
       }
     */
    public async Task<IEnumerable<TicketInfo>> GetUserTickets(string username)
    {
        var ticketsRaw = await ticketService.GetUserTickets(username);
        var flights = (await flightService.GetAllFlightInfos(1, 100)).ToDictionary(x => x.FlightNumber, x => x);
        return ticketsRaw.Select(x => new TicketInfo(
            x.TicketUid,
            x.FlightNumber,
            flights[x.FlightNumber].FromAirport,
            flights[x.FlightNumber].ToAirport,
            flights[x.FlightNumber].Date,
            flights[x.FlightNumber].Price,
            x.Status));
    }
    
    
}
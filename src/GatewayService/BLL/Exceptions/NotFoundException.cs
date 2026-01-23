namespace GatewayService.DTO;

public class NotFoundException(string message) : Exception(message);
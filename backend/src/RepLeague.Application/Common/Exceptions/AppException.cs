namespace RepLeague.Application.Common.Exceptions;

public class AppException(string message) : Exception(message);

public class NotFoundException(string entity, object key)
    : AppException($"{entity} '{key}' was not found.");

public class UnauthorizedException(string message = "Unauthorized.")
    : AppException(message);

public class ConflictException(string message) : AppException(message);

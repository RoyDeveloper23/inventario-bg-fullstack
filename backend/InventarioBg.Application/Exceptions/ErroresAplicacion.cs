namespace InventarioBg.Application.Exceptions;

public sealed class RecursoNoEncontradoException(string mensaje) : Exception(mensaje)
{
}

public sealed class ConflictoAplicacionException(string mensaje) : Exception(mensaje)
{
}

public sealed class CredencialesInvalidasException() : Exception("Credenciales inválidas.")
{
}

namespace CoreSysHM.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entity, int id)
        : base($"{entity} con Id {id} no fue encontrado.") { }
}

public class StockInsuficienteException : DomainException
{
    public StockInsuficienteException(string producto, int stockActual, int cantidadSolicitada)
        : base($"Stock insuficiente para '{producto}'. Disponible: {stockActual}, Solicitado: {cantidadSolicitada}.") { }
}

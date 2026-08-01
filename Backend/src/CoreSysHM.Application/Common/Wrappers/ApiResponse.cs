namespace CoreSysHM.Application.Common.Wrappers;

public class ApiResponse<T>
{
    public bool Exitoso { get; set; }
    public string? Mensaje { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string>? Errores { get; set; }

    public static ApiResponse<T> Success(T data, string? mensaje = null) =>
        new() { Exitoso = true, Data = data, Mensaje = mensaje };

    public static ApiResponse<T> Failure(string mensaje, IEnumerable<string>? errores = null) =>
        new() { Exitoso = false, Mensaje = mensaje, Errores = errores };
}

using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Reportes;

namespace CoreSysHM.Application.Interfaces.Services;

public interface IReporteComprasService
{
    Task<ApiResponse<IEnumerable<ComprasPorPeriodoDto>>> ComprasPorPeriodoAsync(
        DateTime desde, DateTime hasta, string granularidad);

    Task<ApiResponse<IEnumerable<RankingProveedorDto>>> RankingProveedoresAsync(
        DateTime desde, DateTime hasta, int topN);

    Task<ApiResponse<IEnumerable<ProductoMasCompradoDto>>> ProductosMasCompradosAsync(
        DateTime desde, DateTime hasta, int topN, string ordenarPor);

    Task<ApiResponse<IEnumerable<EvolucionPrecioCompraDto>>> EvolucionPrecioCompraAsync(
        int productoId, DateTime desde, DateTime hasta);

    Task<ApiResponse<IEnumerable<SugerenciaReposicionDto>>> SugerenciasReposicionAsync(
        int? proveedorId);
}

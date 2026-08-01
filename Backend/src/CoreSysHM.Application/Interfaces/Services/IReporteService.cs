using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Reportes;

namespace CoreSysHM.Application.Interfaces.Services;

public interface IReporteService
{
    Task<ApiResponse<IEnumerable<ProductoMasVendidoDto>>> ProductosMasVendidosAsync(
        DateTime desde, DateTime hasta, int topN, string ordenarPor);

    Task<ApiResponse<IEnumerable<VentasPorPeriodoDto>>> VentasPorPeriodoAsync(
        DateTime desde, DateTime hasta, string granularidad);

    Task<ApiResponse<IEnumerable<RankingClienteDto>>> RankingClientesAsync(
        DateTime desde, DateTime hasta, int topN);

    Task<ApiResponse<TicketPromedioDto>> TicketPromedioAsync(
        DateTime desde, DateTime hasta);

    Task<ApiResponse<IEnumerable<MargenProductoDto>>> MargenProductosAsync(
        DateTime desde, DateTime hasta, int topN);
}

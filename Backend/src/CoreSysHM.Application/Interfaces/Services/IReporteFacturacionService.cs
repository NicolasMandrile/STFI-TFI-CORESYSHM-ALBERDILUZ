using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Facturacion;
using CoreSysHM.Application.DTOs.Reportes;

namespace CoreSysHM.Application.Interfaces.Services;

public interface IReporteFacturacionService
{
    Task<ApiResponse<IEnumerable<FacturacionPorPeriodoDto>>> FacturacionPorPeriodoAsync(
        DateTime desde, DateTime hasta, string granularidad, int? puntoVentaId, int? tipoComprobanteId);

    Task<ApiResponse<IEnumerable<DesempenoClienteDto>>> DesempenoPorClienteAsync(
        DateTime desde, DateTime hasta, int topN, int? clienteId);

    Task<ApiResponse<IEnumerable<DesempenoProductoDto>>> DesempenoPorProductoAsync(
        DateTime desde, DateTime hasta, int topN, int? productoId);

    /// <summary>Reutiliza IFacturaService.GetVentasFacturablesAsync -- misma fuente que "Nueva Factura", desviación 0 garantizada.</summary>
    Task<ApiResponse<IEnumerable<VentaFacturableDto>>> CarteraPorFacturarAsync(int? clienteId);
}

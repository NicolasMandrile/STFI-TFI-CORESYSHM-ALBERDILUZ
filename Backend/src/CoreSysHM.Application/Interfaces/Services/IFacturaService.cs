using CoreSysHM.Application.Common.Wrappers;
using CoreSysHM.Application.DTOs.Facturacion;

namespace CoreSysHM.Application.Interfaces.Services;

public interface IFacturaService
{
    Task<ApiResponse<IEnumerable<FacturaDto>>> GetAllAsync();
    Task<ApiResponse<FacturaDto>> GetByIdAsync(int id);
    Task<ApiResponse<FacturaDto>> EmitirFacturaAsync(CreateFacturaDto dto, int? usuarioId);
    Task<ApiResponse<bool>> MarcarPagadaAsync(int id);
    Task<ApiResponse<bool>> AnularAsync(int id, int? usuarioId);
    Task<ApiResponse<IEnumerable<FacturaDto>>> GetVencidasAsync();

    /// <summary>
    /// Ventas Confirmadas con saldo pendiente de facturar (total o parcial). Sin filtro de cliente
    /// devuelve todas; usado tanto por la pantalla "Nueva Factura" como por el reporte de cartera.
    /// </summary>
    Task<ApiResponse<IEnumerable<VentaFacturableDto>>> GetVentasFacturablesAsync(int? clienteId = null);

    Task<ApiResponse<VentaFacturableDto>> GetSaldoFacturarAsync(int ventaId);

    Task<ApiResponse<IEnumerable<TipoComprobanteDto>>> GetTiposComprobanteAsync();
    Task<ApiResponse<IEnumerable<PuntoVentaDto>>> GetPuntosVentaAsync();
}

export interface Producto {
  id: number;
  codigo: string;
  nombre: string;
  descripcion?: string;
  precioCompra: number;
  precioVenta: number;
  stockActual: number;
  stockMinimo: number;
  categoriaId: number;
  categoriaNombre: string;
  proveedorId?: number;
  proveedorNombre?: string;
  stockBajo: boolean;
}

export interface CreateProducto {
  codigo: string;
  nombre: string;
  descripcion?: string;
  precioCompra: number;
  precioVenta: number;
  stockMinimo: number;
  stockActual: number;
  categoriaId: number;
  proveedorId?: number;
}

export interface UpdateProducto {
  nombre: string;
  descripcion?: string;
  precioCompra: number;
  precioVenta: number;
  stockMinimo: number;
  categoriaId: number;
  proveedorId?: number;
}

export interface AjustarStock {
  cantidad: number;
  tipoMovimiento: string;
  observacion?: string;
}

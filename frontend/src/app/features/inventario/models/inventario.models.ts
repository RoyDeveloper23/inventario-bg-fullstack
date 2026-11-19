export interface ProductoInventario {
  id: string;
  productoId: string;
  proveedorId: string;
  proveedorNombre: string;
  numeroLote: string;
  precioUnitario: number;
  stockDisponible: number;
  fechaIngreso: string;
}

export interface CrearProductoInventarioRequest {
  proveedorId: string;
  numeroLote: string;
  precioUnitario: number;
  stockDisponible: number;
}

export interface ActualizarProductoInventarioRequest {
  precioUnitario: number;
  stockDisponible: number;
}

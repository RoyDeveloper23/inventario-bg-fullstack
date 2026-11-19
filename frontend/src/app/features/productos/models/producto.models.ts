export interface Producto {
  id: string;
  codigoSku: string;
  nombre: string;
  descripcion: string | null;
  categoria: string | null;
  activo: boolean;
}

export interface ProductoListItem extends Producto {
  stockTotal: number;
}

export interface ProductoRequest {
  codigoSku: string;
  nombre: string;
  descripcion: string | null;
  categoria: string | null;
}

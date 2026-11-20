import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { environment } from '../../../../environments/environment';
import { PagedResult } from '../../../core/models/paged-result.models';
import {
  Producto,
  ProductoListItem,
  ProductoRequest
} from '../models/producto.models';

@Injectable({ providedIn: 'root' })
export class ProductoService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/productos`;

  listar(
    busqueda: string | null = null,
    proveedorId: string | null = null,
    categoria: string | null = null,
    activo: boolean | null = true,
    page = 1,
    pageSize = 20
  ) {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (busqueda?.trim()) {
      params = params.set('busqueda', busqueda.trim());
    }
    if (proveedorId) {
      params = params.set('proveedorId', proveedorId);
    }
    if (categoria?.trim()) {
      params = params.set('categoria', categoria.trim());
    }
    params = params.set('activo', activo === null ? '' : String(activo));

    return this.http.get<PagedResult<ProductoListItem>>(this.url, { params });
  }

  obtener(id: string) {
    return this.http.get<Producto>(`${this.url}/${id}`);
  }

  crear(datos: ProductoRequest) {
    return this.http.post<Producto>(this.url, datos);
  }

  actualizar(id: string, datos: ProductoRequest) {
    return this.http.put<Producto>(`${this.url}/${id}`, datos);
  }

  desactivar(id: string) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  eliminarPermanente(id: string) {
    return this.http.delete<void>(`${this.url}/${id}/permanente`);
  }

  reactivar(id: string) {
    return this.http.post<Producto>(`${this.url}/${id}/reactivar`, {});
  }
}

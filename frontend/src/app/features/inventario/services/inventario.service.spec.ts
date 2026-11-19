import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../../environments/environment';
import {
  ActualizarProductoInventarioRequest,
  CrearProductoInventarioRequest,
  ProductoInventario
} from '../models/inventario.models';
import { InventarioService } from './inventario.service';

describe('InventarioService', () => {
  let service: InventarioService;
  let http: HttpTestingController;
  const url = `${environment.apiUrl}/productos/producto-1/inventarios`;
  const inventario: ProductoInventario = {
    id: 'lote-1',
    productoId: 'producto-1',
    proveedorId: 'proveedor-1',
    proveedorNombre: 'Proveedor Uno',
    numeroLote: 'LOTE-1',
    precioUnitario: 14.5,
    stockDisponible: 12,
    fechaIngreso: '2026-11-18T18:00:00Z'
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        InventarioService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(InventarioService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('consulta las existencias por producto', () => {
    service.listar('producto-1').subscribe((resultado) => {
      expect(resultado.items).toEqual([inventario]);
    });

    const request = http.expectOne(url);
    expect(request.request.method).toBe('GET');
    request.flush({ items: [inventario] });
  });

  it('crea, actualiza y elimina lotes con las rutas y solicitudes del contrato', () => {
    const crear: CrearProductoInventarioRequest = {
      proveedorId: 'proveedor-1',
      numeroLote: 'LOTE-1',
      precioUnitario: 14.5,
      stockDisponible: 12
    };
    service.crear('producto-1', crear).subscribe();
    const crearRequest = http.expectOne(url);
    expect(crearRequest.request.method).toBe('POST');
    expect(crearRequest.request.body).toEqual(crear);
    crearRequest.flush(inventario);

    const actualizar: ActualizarProductoInventarioRequest = {
      precioUnitario: 15.25,
      stockDisponible: 10
    };
    service.actualizar('producto-1', 'lote-1', actualizar).subscribe();
    const actualizarRequest = http.expectOne(`${url}/lote-1`);
    expect(actualizarRequest.request.method).toBe('PUT');
    expect(actualizarRequest.request.body).toEqual(actualizar);
    actualizarRequest.flush({ ...inventario, ...actualizar });

    service.eliminar('producto-1', 'lote-1').subscribe();
    const eliminarRequest = http.expectOne(`${url}/lote-1`);
    expect(eliminarRequest.request.method).toBe('DELETE');
    eliminarRequest.flush(null);
  });
});

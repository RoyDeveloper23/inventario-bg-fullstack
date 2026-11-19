import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../../environments/environment';
import { ProductoService } from './producto.service';

describe('ProductoService', () => {
  let service: ProductoService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ProductoService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ProductoService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('lista productos con filtros y paginacion', () => {
    service.listar('botella', 'proveedor-1', 2, 10).subscribe();

    const request = http.expectOne((req) =>
      req.url === `${environment.apiUrl}/productos`
      && req.params.get('busqueda') === 'botella'
      && req.params.get('proveedorId') === 'proveedor-1'
      && req.params.get('page') === '2'
      && req.params.get('pageSize') === '10');
    expect(request.request.method).toBe('GET');
    request.flush({ items: [], totalCount: 0, page: 2, pageSize: 10 });
  });

  it('crea, actualiza y desactiva un producto', () => {
    const datos = { codigoSku: 'SKU-1', nombre: 'Producto', descripcion: null, categoria: null };

    service.crear(datos).subscribe();
    http.expectOne(`${environment.apiUrl}/productos`).flush({
      id: '1', codigoSku: 'SKU-1', nombre: 'Producto', descripcion: null, categoria: null, activo: true
    });

    service.actualizar('1', datos).subscribe();
    http.expectOne(`${environment.apiUrl}/productos/1`).flush({
      id: '1', codigoSku: 'SKU-1', nombre: 'Producto', descripcion: null, categoria: null, activo: true
    });

    service.desactivar('1').subscribe();
    http.expectOne(`${environment.apiUrl}/productos/1`).flush(null);
  });
});

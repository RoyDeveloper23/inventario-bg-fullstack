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
    service.listar('botella', 'proveedor-1', 'Bebidas', false, 2, 10).subscribe();

    const request = http.expectOne((req) =>
      req.url === `${environment.apiUrl}/productos`
      && req.params.get('busqueda') === 'botella'
      && req.params.get('proveedorId') === 'proveedor-1'
      && req.params.get('categoria') === 'Bebidas'
      && req.params.get('activo') === 'false'
      && req.params.get('page') === '2'
      && req.params.get('pageSize') === '10');
    expect(request.request.method).toBe('GET');
    request.flush({ items: [], totalCount: 0, page: 2, pageSize: 10 });
  });

  it('crea, actualiza, desactiva, elimina y reactiva un producto', () => {
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
    const desactivarRequest = http.expectOne({
      method: 'DELETE',
      url: `${environment.apiUrl}/productos/1`
    });
    expect(desactivarRequest.request.method).toBe('DELETE');
    desactivarRequest.flush(null);

    service.eliminarPermanente('1').subscribe();
    const eliminarRequest = http.expectOne({
      method: 'DELETE',
      url: `${environment.apiUrl}/productos/1/permanente`
    });
    eliminarRequest.flush(null);

    service.reactivar('1').subscribe();
    const reactivarRequest = http.expectOne({
      method: 'POST',
      url: `${environment.apiUrl}/productos/1/reactivar`
    });
    expect(reactivarRequest.request.method).toBe('POST');
    reactivarRequest.flush({
      id: '1', codigoSku: 'SKU-1', nombre: 'Producto', descripcion: null, categoria: null, activo: true
    });
  });

  it('envia activo vacio para mostrar todos', () => {
    service.listar(null, null, null, null).subscribe();

    const request = http.expectOne((req) =>
      req.url === `${environment.apiUrl}/productos`
      && req.params.has('activo')
      && req.params.get('page') === '1'
      && req.params.get('pageSize') === '20');
    expect(request.request.params.get('activo')).toBe('');
    request.flush({ items: [], totalCount: 0, page: 1, pageSize: 20 });
  });

  it('usa activo=true si no se selecciona otro estado', () => {
    service.listar().subscribe();

    const request = http.expectOne((req) =>
      req.url === `${environment.apiUrl}/productos`
      && req.params.has('activo'));
    expect(request.request.params.get('activo')).toBe('true');
    request.flush({ items: [], totalCount: 0, page: 1, pageSize: 20 });
  });
});

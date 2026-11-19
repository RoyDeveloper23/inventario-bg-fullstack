import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { ProveedorService } from './proveedor.service';

describe('ProveedorService', () => {
  let service: ProveedorService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ProveedorService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ProveedorService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('consulta proveedores y conserva el contrato de respuesta', () => {
    let resultado: { items: { id: string; nombre: string }[] } | undefined;
    service.listar().subscribe((respuesta) => resultado = respuesta);

    const request = http.expectOne(`${environment.apiUrl}/proveedores`);
    expect(request.request.method).toBe('GET');
    request.flush({ items: [{ id: 'proveedor-1', nombre: 'Proveedor Uno' }] });

    expect(resultado).toEqual({ items: [{ id: 'proveedor-1', nombre: 'Proveedor Uno' }] });
  });
});

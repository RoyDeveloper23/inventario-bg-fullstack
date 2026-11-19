import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';

import { environment } from '../../../../environments/environment';
import { ProductFormComponent } from './product-form.component';

describe('ProductFormComponent', () => {
  let http: HttpTestingController;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductFormComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({}) } }
        }
      ]
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.resolveTo(true);
  });

  afterEach(() => http.verify());

  it('marca campos requeridos como tocados', () => {
    const fixture = TestBed.createComponent(ProductFormComponent);
    fixture.detectChanges();

    fixture.componentInstance.guardar();

    expect(fixture.componentInstance.formulario.controls.codigoSku.touched).toBeTrue();
    expect(fixture.componentInstance.formulario.controls.nombre.touched).toBeTrue();
  });

  it('crea un producto valido', () => {
    const fixture = TestBed.createComponent(ProductFormComponent);
    fixture.detectChanges();
    fixture.componentInstance.formulario.setValue({
      codigoSku: 'SKU-1',
      nombre: 'Producto',
      descripcion: '',
      categoria: ''
    });

    fixture.componentInstance.guardar();

    const request = http.expectOne(`${environment.apiUrl}/productos`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      codigoSku: 'SKU-1',
      nombre: 'Producto',
      descripcion: null,
      categoria: null
    });
    request.flush({
      id: '1', codigoSku: 'SKU-1', nombre: 'Producto', descripcion: null, categoria: null, activo: true
    });
    expect(router.navigate).toHaveBeenCalledWith(['/productos']);
  });
});

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../../environments/environment';
import { ProductoInventario } from '../models/inventario.models';
import { InventoryFormComponent } from './inventory-form.component';

describe('InventoryFormComponent', () => {
  let http: HttpTestingController;

  const inventario: ProductoInventario = {
    id: 'lote-1',
    productoId: 'producto-1',
    proveedorId: 'proveedor-1',
    proveedorNombre: 'Proveedor Uno',
    numeroLote: 'LOTE-1',
    precioUnitario: 14.5,
    stockDisponible: 0,
    fechaIngreso: '2026-11-18T18:00:00Z'
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InventoryFormComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function crearFormulario(inventarioInicial: ProductoInventario | null = null) {
    const fixture = TestBed.createComponent(InventoryFormComponent);
    fixture.componentRef.setInput('productoId', 'producto-1');
    fixture.componentRef.setInput('inventario', inventarioInicial);
    fixture.detectChanges();

    if (!inventarioInicial) {
      http.expectOne(`${environment.apiUrl}/proveedores`)
        .flush({ items: [{ id: 'proveedor-1', nombre: 'Proveedor Uno' }] });
    }

    return fixture;
  }

  it('valida campos requeridos, precio y stock no negativos', () => {
    const fixture = crearFormulario();
    const formulario = fixture.componentInstance.formulario;
    formulario.setValue({
      proveedorId: '',
      numeroLote: '',
      precioUnitario: -1,
      stockDisponible: -1
    });

    fixture.componentInstance.guardar();

    expect(formulario.invalid).toBeTrue();
    expect(formulario.controls.proveedorId.touched).toBeTrue();
    expect(formulario.controls.precioUnitario.hasError('min')).toBeTrue();
    expect(formulario.controls.stockDisponible.hasError('min')).toBeTrue();
  });

  it('crea un lote válido con el contrato de alta', () => {
    const fixture = crearFormulario();
    const completado = spyOn(fixture.componentInstance.completado, 'emit');
    fixture.componentInstance.formulario.setValue({
      proveedorId: 'proveedor-1',
      numeroLote: ' LOTE-1 ',
      precioUnitario: 14.5,
      stockDisponible: 12
    });

    fixture.componentInstance.guardar();

    const request = http.expectOne(`${environment.apiUrl}/productos/producto-1/inventarios`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      proveedorId: 'proveedor-1',
      numeroLote: 'LOTE-1',
      precioUnitario: 14.5,
      stockDisponible: 12
    });
    request.flush(inventario);
    expect(completado).toHaveBeenCalled();
  });

  it('actualiza solo precio y stock y limita la eliminación al stock cero confirmado', () => {
    const fixture = crearFormulario(inventario);
    const completar = spyOn(fixture.componentInstance.completado, 'emit');
    fixture.componentInstance.formulario.controls.precioUnitario.setValue(15.25);
    fixture.componentInstance.formulario.controls.stockDisponible.setValue(10);

    fixture.componentInstance.guardar();

    const actualizar = http.expectOne(
      `${environment.apiUrl}/productos/producto-1/inventarios/lote-1`
    );
    expect(actualizar.request.method).toBe('PUT');
    expect(actualizar.request.body).toEqual({ precioUnitario: 15.25, stockDisponible: 10 });
    actualizar.flush({ ...inventario, precioUnitario: 15.25, stockDisponible: 10 });

    spyOn(window, 'confirm').and.returnValue(false);
    fixture.componentInstance.eliminar();
    http.expectNone(`${environment.apiUrl}/productos/producto-1/inventarios/lote-1`);

    (window.confirm as jasmine.Spy).and.returnValue(true);
    fixture.componentInstance.eliminar();
    const eliminar = http.expectOne(
      `${environment.apiUrl}/productos/producto-1/inventarios/lote-1`
    );
    expect(eliminar.request.method).toBe('DELETE');
    eliminar.flush(null);
    expect(completar).toHaveBeenCalledTimes(2);
  });

  it('muestra error y permite reintentar la consulta de proveedores', () => {
    const fixture = TestBed.createComponent(InventoryFormComponent);
    fixture.componentRef.setInput('productoId', 'producto-1');
    fixture.detectChanges();
    http.expectOne(`${environment.apiUrl}/proveedores`)
      .flush({}, { status: 500, statusText: 'Error del servidor' });

    expect(fixture.componentInstance.errorProveedores).toBeTrue();

    fixture.componentInstance.cargarProveedores();
    http.expectOne(`${environment.apiUrl}/proveedores`)
      .flush({ items: [{ id: 'proveedor-1', nombre: 'Proveedor Uno' }] });
    expect(fixture.componentInstance.errorProveedores).toBeFalse();
  });
});

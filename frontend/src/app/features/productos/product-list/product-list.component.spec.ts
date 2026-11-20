import { provideHttpClient } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { NotificationService } from '../../../core/services/notification.service';
import { ProveedorService } from '../../../core/services/proveedor.service';
import { ProductoService } from '../services/producto.service';
import { ProductListComponent } from './product-list.component';

describe('ProductListComponent', () => {
  let productoService: jasmine.SpyObj<ProductoService>;
  let proveedorService: jasmine.SpyObj<ProveedorService>;
  let notifications: jasmine.SpyObj<NotificationService>;
  let fixture: ComponentFixture<ProductListComponent>;
  let component: ProductListComponent;

  beforeEach(() => {
    productoService = jasmine.createSpyObj<ProductoService>(
      'ProductoService',
      ['listar', 'desactivar', 'eliminarPermanente', 'reactivar']
    );
    productoService.listar.and.returnValue(of({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 20
    }));
    productoService.desactivar.and.returnValue(of(undefined));
    productoService.eliminarPermanente.and.returnValue(of(undefined));
    productoService.reactivar.and.returnValue(of({
      id: '1',
      codigoSku: 'SKU-1',
      nombre: 'Producto',
      descripcion: null,
      categoria: null,
      activo: true
    }));

    proveedorService = jasmine.createSpyObj<ProveedorService>(
      'ProveedorService',
      ['listar']
    );
    proveedorService.listar.and.returnValue(of({ items: [] }));
    notifications = jasmine.createSpyObj<NotificationService>(
      'NotificationService',
      ['mostrarError', 'mostrarExito']
    );

    TestBed.configureTestingModule({
      imports: [ProductListComponent],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        { provide: ProductoService, useValue: productoService },
        { provide: ProveedorService, useValue: proveedorService },
        { provide: NotificationService, useValue: notifications }
      ]
    });

    fixture = TestBed.createComponent(ProductListComponent);
    component = fixture.componentInstance;
  });

  it('no busca otra vez sin cambios respecto a la ultima ejecucion', () => {
    component.ngOnInit();

    component.buscar();
    component.limpiarFiltros();

    expect(productoService.listar).toHaveBeenCalledTimes(1);
  });

  it('busca al cambiar categoria, estado y proveedor', () => {
    component.ngOnInit();
    component.filtros.patchValue({
      categoria: 'Bebidas',
      estado: 'inactivos',
      proveedorId: 'proveedor-1'
    });

    component.buscar();

    expect(productoService.listar).toHaveBeenCalledWith(
      '',
      'proveedor-1',
      'Bebidas',
      false,
      1,
      5
    );
  });

  it('desactiva localmente y no vuelve a consultar la lista', () => {
    fixture.detectChanges();
    expect(component.filtros.controls.estado.value).toBe('todos');
    expect((fixture.nativeElement.querySelector('select[formControlName="estado"]') as HTMLSelectElement).value)
      .toBe('todos');
    expect(productoService.listar.calls.argsFor(0)[3]).toBeNull();
    const producto = {
      id: '1',
      codigoSku: 'SKU-1',
      nombre: 'Producto',
      descripcion: null,
      categoria: null,
      activo: true,
      stockTotal: 7,
      proveedores: ['Proveedor Uno']
    };
    component.productos = [producto];
    component.total = 1;

    component.desactivar(producto);
    expect(component.productoParaConfirmar).toEqual(producto);
    component.confirmarCambioEstado();
    fixture.detectChanges();

    expect(productoService.desactivar).toHaveBeenCalledOnceWith('1');
    expect(component.productos).toEqual([{ ...producto, activo: false }]);
    expect(component.total).toBe(1);
    expect(fixture.nativeElement.querySelector('tbody .pill')?.textContent.trim()).toBe('Inactivo');
    expect(productoService.listar).toHaveBeenCalledTimes(1);
  });

  it('cambia el tamano de pagina y consulta desde la primera pagina', () => {
    component.ngOnInit();
    component.pagina = 3;

    component.cambiarTamanoPagina('25');

    expect(component.tamanoPagina).toBe(25);
    expect(component.pagina).toBe(1);
    expect(productoService.listar).toHaveBeenCalledTimes(2);
    expect(productoService.listar.calls.argsFor(1)).toEqual([
      '',
      null,
      '',
      null,
      1,
      25
    ]);
  });

  it('actualiza el estado local al reactivar en el filtro de todos', () => {
    fixture.detectChanges();
    component.filtros.controls.estado.setValue('todos');
    const producto = {
      id: '1',
      codigoSku: 'SKU-1',
      nombre: 'Producto',
      descripcion: null,
      categoria: null,
      activo: false,
      stockTotal: 0,
      proveedores: []
    };
    component.productos = [producto];

    component.reactivar(producto);
    fixture.detectChanges();

    expect(component.productoParaConfirmar).toEqual(producto);
    expect(fixture.nativeElement.querySelector('[role="dialog"]')?.textContent).toContain('Activar producto');
    component.confirmarCambioEstado();
    expect(productoService.reactivar).toHaveBeenCalledOnceWith('1');
    expect(component.productos[0].activo).toBeTrue();
    expect(component.productoParaConfirmar).toBeNull();
    expect(productoService.listar).toHaveBeenCalledTimes(1);
  });

  it('elimina localmente un producto sin lotes y muestra confirmacion', () => {
    fixture.detectChanges();
    const producto = {
      id: '2',
      codigoSku: 'SKU-2',
      nombre: 'Producto sin lotes',
      descripcion: null,
      categoria: null,
      activo: true,
      stockTotal: 0,
      proveedores: []
    };
    component.productos = [producto];
    component.total = 1;

    component.eliminarPermanentemente(producto);
    fixture.detectChanges();

    const dialogo = fixture.nativeElement.querySelector('[role="dialog"]') as HTMLElement;
    expect(dialogo.textContent).toContain('Esta acción no se puede deshacer.');
    component.confirmarCambioEstado();
    fixture.detectChanges();

    expect(productoService.eliminarPermanente).toHaveBeenCalledOnceWith('2');
    expect(component.productos).toEqual([]);
    expect(component.total).toBe(0);
    expect(component.productoParaConfirmar).toBeNull();
    expect(notifications.mostrarExito).toHaveBeenCalledOnceWith('Producto eliminado permanentemente.');
    expect(productoService.listar).toHaveBeenCalledTimes(1);
  });

  it('mantiene Eliminar alineado y lo deshabilita con tooltip si hay lotes de stock cero', () => {
    fixture.detectChanges();
    component.productos = [
      {
        id: '3',
        codigoSku: 'SKU-3',
        nombre: 'Producto con lote',
        descripcion: null,
        categoria: null,
        activo: true,
        stockTotal: 0,
        proveedores: ['Proveedor Uno']
      },
      {
        id: '4',
        codigoSku: 'SKU-4',
        nombre: 'Producto sin lote',
        descripcion: null,
        categoria: null,
        activo: true,
        stockTotal: 0,
        proveedores: []
      }
    ];
    component.total = 2;
    fixture.detectChanges();

    const botones = fixture.nativeElement.querySelectorAll(
      'tbody .eliminar-producto'
    ) as NodeListOf<HTMLButtonElement>;
    expect(botones.length).toBe(2);
    expect(botones[0].disabled).toBeTrue();
    expect(botones[0].hasAttribute('title')).toBeFalse();
    expect(botones[0].classList).toContain('opacity-45');
    expect(botones[0].classList).toContain('cursor-not-allowed');
    const tooltip = botones[0].parentElement?.querySelector('[role="tooltip"]') as HTMLElement;
    expect(tooltip.textContent?.trim()).toBe('No se puede eliminar porque tiene lotes registrados');
    expect(tooltip.classList).toContain('group-hover:opacity-100');
    expect(tooltip.classList).toContain('group-focus-within:opacity-100');
    expect(botones[0].parentElement?.classList).toContain('group');
    expect(botones[0].parentElement?.classList).toContain('cursor-not-allowed');
    expect(tooltip.classList).toContain('whitespace-normal');
    expect(tooltip.classList).toContain('break-words');
    expect(botones[1].disabled).toBeFalse();
    expect(botones[1].parentElement?.querySelector('[role="tooltip"]')).toBeNull();
  });
});

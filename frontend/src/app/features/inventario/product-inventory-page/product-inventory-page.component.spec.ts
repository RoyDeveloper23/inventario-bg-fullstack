import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { NotificationService } from '../../../core/services/notification.service';
import { Producto } from '../../productos/models/producto.models';
import { ProductoService } from '../../productos/services/producto.service';
import { ProductoInventario } from '../models/inventario.models';
import { InventarioService } from '../services/inventario.service';
import { ProductInventoryPageComponent } from './product-inventory-page.component';

describe('ProductInventoryPageComponent', () => {
  it('calcula el stock total y cuenta proveedores distintos', () => {
    TestBed.configureTestingModule({
      imports: [ProductInventoryPageComponent],
      providers: [
        provideRouter([]),
        { provide: ProductoService, useValue: {} },
        { provide: InventarioService, useValue: {} },
        { provide: NotificationService, useValue: {} }
      ]
    });

    const component = TestBed.createComponent(ProductInventoryPageComponent).componentInstance;
    component.inventarios = [
      {
        id: 'lote-1',
        productoId: 'producto-1',
        proveedorId: 'proveedor-1',
        proveedorNombre: 'Proveedor Uno',
        numeroLote: 'LOTE-1',
        precioUnitario: 12,
        stockDisponible: 5,
        fechaIngreso: '2026-11-19'
      },
      {
        id: 'lote-2',
        productoId: 'producto-1',
        proveedorId: 'proveedor-1',
        proveedorNombre: 'Proveedor Uno',
        numeroLote: 'LOTE-2',
        precioUnitario: 14,
        stockDisponible: 3,
        fechaIngreso: '2026-11-19'
      },
      {
        id: 'lote-3',
        productoId: 'producto-1',
        proveedorId: 'proveedor-2',
        proveedorNombre: 'Proveedor Dos',
        numeroLote: 'LOTE-3',
        precioUnitario: 15,
        stockDisponible: 7,
        fechaIngreso: '2026-11-19'
      }
    ] satisfies ProductoInventario[];

    expect(component.stockDisponibleTotal).toBe(15);
    expect(component.proveedoresCount).toBe(2);
  });

  it('confirma la eliminacion y actualiza la lista y los indicadores localmente', () => {
    const productoService = jasmine.createSpyObj<ProductoService>('ProductoService', ['obtener']);
    const inventarioService = jasmine.createSpyObj<InventarioService>(
      'InventarioService',
      ['listar', 'eliminar']
    );
    const notifications = jasmine.createSpyObj<NotificationService>(
      'NotificationService',
      ['mostrarExito']
    );
    const producto: Producto = {
      id: 'producto-1',
      codigoSku: 'SKU-1',
      nombre: 'Producto',
      descripcion: null,
      categoria: 'Accesorios',
      activo: true
    };
    const lotes: ProductoInventario[] = [
      {
        id: 'lote-1',
        productoId: 'producto-1',
        proveedorId: 'proveedor-1',
        proveedorNombre: 'Proveedor Uno',
        numeroLote: 'LOTE-1',
        precioUnitario: 12,
        stockDisponible: 8,
        fechaIngreso: '2026-11-19'
      },
      {
        id: 'lote-2',
        productoId: 'producto-1',
        proveedorId: 'proveedor-2',
        proveedorNombre: 'Proveedor Dos',
        numeroLote: 'LOTE-2',
        precioUnitario: 14,
        stockDisponible: 4,
        fechaIngreso: '2026-11-19'
      }
    ];
    productoService.obtener.and.returnValue(of(producto));
    inventarioService.listar.and.returnValue(of({
      items: lotes,
      totalCount: lotes.length,
      page: 1,
      pageSize: 10
    }));
    inventarioService.eliminar.and.returnValue(of(undefined));

    TestBed.configureTestingModule({
      imports: [ProductInventoryPageComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: 'producto-1' }) } }
        },
        { provide: ProductoService, useValue: productoService },
        { provide: InventarioService, useValue: inventarioService },
        { provide: NotificationService, useValue: notifications }
      ]
    });

    const fixture = TestBed.createComponent(ProductInventoryPageComponent);
    fixture.detectChanges();

    const eliminar = Array.from(
      fixture.nativeElement.querySelectorAll('.actions button') as NodeListOf<HTMLButtonElement>
    ).find((button) => button.textContent?.trim() === 'Eliminar');
    expect(eliminar).toBeDefined();
    eliminar?.click();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.confirm-dialog')?.textContent)
      .toContain('Esta acción descontará su cantidad del stock disponible.');

    const confirmar = fixture.nativeElement.querySelector(
      '.confirm-dialog button:not(.btn-ghost)'
    ) as HTMLButtonElement;
    confirmar.click();
    fixture.detectChanges();

    expect(inventarioService.eliminar).toHaveBeenCalledOnceWith('producto-1', 'lote-1');
    expect(fixture.componentInstance.inventarios.map((lote) => lote.id)).toEqual(['lote-2']);
    expect(fixture.componentInstance.stockDisponibleTotal).toBe(4);
    expect(fixture.componentInstance.proveedoresCount).toBe(1);
    expect(fixture.nativeElement.querySelectorAll('.stats .stat b')[1].textContent.trim()).toBe('1');
    expect(notifications.mostrarExito)
      .toHaveBeenCalledOnceWith('Lote eliminado; el stock disponible fue actualizado.');
    expect(fixture.nativeElement.querySelector('.confirm-dialog')).toBeNull();
  });
});

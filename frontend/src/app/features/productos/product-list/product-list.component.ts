import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize, map } from 'rxjs';

import { Proveedor } from '../../../core/models/proveedor.models';
import { NotificationService } from '../../../core/services/notification.service';
import { ProveedorService } from '../../../core/services/proveedor.service';
import { ProductoListItem } from '../models/producto.models';
import { ProductoService } from '../services/producto.service';

@Component({
  selector: 'app-product-list',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.css'
})
export class ProductListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly notifications = inject(NotificationService);
  private readonly proveedorService = inject(ProveedorService);
  private readonly productoService = inject(ProductoService);

  readonly filtros = this.fb.nonNullable.group({
    busqueda: '',
    proveedorId: '',
    categoria: '',
    estado: 'todos'
  });

  productos: ProductoListItem[] = [];
  proveedores: Proveedor[] = [];
  pagina = 1;
  tamanoPagina = 10;
  total = 0;
  cargando = false;
  error = false;
  actualizandoEstado = false;
  productoParaConfirmar: ProductoListItem | null = null;
  accionPendiente: 'desactivar' | 'activar' | 'eliminar' = 'desactivar';
  private ultimaBusqueda: string | null = null;

  ngOnInit(): void {
    this.cargarProveedores();
    this.ultimaBusqueda = this.claveFiltros();
    this.cargar();
  }

  buscar(): void {
    const clave = this.claveFiltros();
    if (clave === this.ultimaBusqueda) {
      return;
    }

    this.ultimaBusqueda = clave;
    this.pagina = 1;
    this.cargar();
  }

  limpiarFiltros(): void {
    this.filtros.reset({
      busqueda: '',
      proveedorId: '',
      categoria: '',
      estado: 'todos'
    });
    const clave = this.claveFiltros();
    if (clave === this.ultimaBusqueda && this.pagina === 1) {
      return;
    }

    this.ultimaBusqueda = clave;
    this.pagina = 1;
    this.cargar();
  }

  cambiarPagina(pagina: number): void {
    if (pagina < 1 || pagina > this.totalPaginas) {
      return;
    }
    this.pagina = pagina;
    this.cargar();
  }

  cambiarTamanoPagina(tamano: string): void {
    const nuevoTamano = Number(tamano);
    if (![5, 10, 25, 50].includes(nuevoTamano) || nuevoTamano === this.tamanoPagina) {
      return;
    }

    this.tamanoPagina = nuevoTamano;
    this.pagina = 1;
    this.cargar();
  }

  desactivar(producto: ProductoListItem): void {
    this.solicitarConfirmacion(producto, 'desactivar');
  }

  confirmarCambioEstado(): void {
    const producto = this.productoParaConfirmar;
    if (!producto || this.actualizandoEstado) {
      return;
    }

    const accion = this.accionPendiente;
    const activo = accion === 'activar';
    const operacion = accion === 'activar'
      ? this.productoService.reactivar(producto.id).pipe(map(() => undefined))
      : accion === 'eliminar'
        ? this.productoService.eliminarPermanente(producto.id)
        : this.productoService.desactivar(producto.id);

    this.actualizandoEstado = true;
    operacion.pipe(finalize(() => this.actualizandoEstado = false))
      .subscribe({
        next: () => {
          if (accion === 'eliminar') {
            this.eliminarLocal(producto);
            this.notifications.mostrarExito('Producto eliminado permanentemente.');
          } else {
            this.actualizarEstadoLocal(producto, activo);
          }
          this.productoParaConfirmar = null;
        }
      });
  }

  cancelarCambioEstado(): void {
    if (!this.actualizandoEstado) {
      this.productoParaConfirmar = null;
    }
  }

  reactivar(producto: ProductoListItem): void {
    this.solicitarConfirmacion(producto, 'activar');
  }

  eliminarPermanentemente(producto: ProductoListItem): void {
    this.solicitarConfirmacion(producto, 'eliminar');
  }

  get totalPaginas(): number {
    return Math.max(1, Math.ceil(this.total / this.tamanoPagina));
  }

  private cargarProveedores(): void {
    this.proveedorService.listar().subscribe({
      next: (resultado) => this.proveedores = resultado.items
    });
  }

  cargar(): void {
    this.cargando = true;
    this.error = false;
    const filtros = this.filtros.getRawValue();

    this.productoService.listar(
      filtros.busqueda,
      filtros.proveedorId || null,
      filtros.categoria,
      filtros.estado === 'todos' ? null : filtros.estado === 'activos',
      this.pagina,
      this.tamanoPagina
    ).subscribe({
      next: (resultado) => {
        this.productos = resultado.items;
        this.total = resultado.totalCount;
        this.cargando = false;
      },
      error: () => {
        this.error = true;
        this.cargando = false;
      }
    });
  }

  private claveFiltros(): string {
    const filtros = this.filtros.getRawValue();
    return JSON.stringify({
      busqueda: filtros.busqueda.trim(),
      proveedorId: filtros.proveedorId,
      categoria: filtros.categoria.trim(),
      estado: filtros.estado
    });
  }

  private actualizarEstadoLocal(producto: ProductoListItem, activo: boolean): void {
    if (this.filtros.controls.estado.value === 'todos') {
      this.productos = this.productos.map((item) =>
        item.id === producto.id ? { ...item, activo } : item
      );
      return;
    }

    const cantidadAntes = this.productos.length;
    this.productos = this.productos.filter((item) => item.id !== producto.id);
    if (this.productos.length !== cantidadAntes) {
      this.total = Math.max(0, this.total - 1);
    }
  }

  private eliminarLocal(producto: ProductoListItem): void {
    const cantidadAntes = this.productos.length;
    this.productos = this.productos.filter((item) => item.id !== producto.id);
    if (this.productos.length !== cantidadAntes) {
      this.total = Math.max(0, this.total - 1);
    }
  }

  private solicitarConfirmacion(
    producto: ProductoListItem,
    accion: 'desactivar' | 'activar' | 'eliminar'
  ): void {
    this.productoParaConfirmar = producto;
    this.accionPendiente = accion;
  }
}

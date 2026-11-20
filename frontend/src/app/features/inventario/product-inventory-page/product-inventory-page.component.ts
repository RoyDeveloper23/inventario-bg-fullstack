import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';

import { NotificationService } from '../../../core/services/notification.service';
import { Producto } from '../../productos/models/producto.models';
import { ProductoService } from '../../productos/services/producto.service';
import { InventoryFormComponent } from '../inventory-form/inventory-form.component';
import { ProductoInventario } from '../models/inventario.models';
import { InventarioService } from '../services/inventario.service';

@Component({
  selector: 'app-product-inventory-page',
  imports: [CommonModule, InventoryFormComponent, RouterLink],
  templateUrl: './product-inventory-page.component.html',
  styleUrl: './product-inventory-page.component.css'
})
export class ProductInventoryPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly productoService = inject(ProductoService);
  private readonly inventarioService = inject(InventarioService);
  private readonly notifications = inject(NotificationService);

  productoId = '';
  producto: Producto | null = null;
  inventarios: ProductoInventario[] = [];
  cargando = false;
  error = false;
  mostrarFormulario = false;
  inventarioEnEdicion: ProductoInventario | null = null;
  inventarioParaEliminar: ProductoInventario | null = null;
  eliminandoInventario = false;

  get stockDisponibleTotal(): number {
    return this.inventarios.reduce((total, inventario) => total + inventario.stockDisponible, 0);
  }

  get proveedoresCount(): number {
    return new Set(this.inventarios.map((inventario) => inventario.proveedorId)).size;
  }

  ngOnInit(): void {
    this.productoId = this.route.snapshot.paramMap.get('id') ?? '';
    this.cargar();
  }

  cargar(): void {
    if (!this.productoId) {
      this.error = true;
      return;
    }

    this.cargando = true;
    this.error = false;
    forkJoin({
      producto: this.productoService.obtener(this.productoId),
      resultado: this.inventarioService.listar(this.productoId)
    }).subscribe({
      next: ({ producto, resultado }) => {
        this.producto = producto;
        this.inventarios = resultado.items;
        this.cargando = false;
      },
      error: () => {
        this.error = true;
        this.cargando = false;
      }
    });
  }

  nuevoLote(): void {
    this.inventarioEnEdicion = null;
    this.mostrarFormulario = true;
  }

  editar(inventario: ProductoInventario): void {
    this.inventarioEnEdicion = inventario;
    this.mostrarFormulario = true;
  }

  solicitarEliminacion(inventario: ProductoInventario): void {
    this.inventarioParaEliminar = inventario;
  }

  cancelarEliminacion(): void {
    if (!this.eliminandoInventario) {
      this.inventarioParaEliminar = null;
    }
  }

  confirmarEliminacion(): void {
    const inventario = this.inventarioParaEliminar;
    if (!inventario || this.eliminandoInventario) {
      return;
    }

    this.eliminandoInventario = true;
    this.inventarioService.eliminar(this.productoId, inventario.id)
      .pipe(finalize(() => this.eliminandoInventario = false))
      .subscribe({
        next: () => {
          this.inventarios = this.inventarios.filter((item) => item.id !== inventario.id);
          this.inventarioParaEliminar = null;
          this.notifications.mostrarExito('Lote eliminado; el stock disponible fue actualizado.');
        }
      });
  }

  cerrarFormulario(): void {
    this.mostrarFormulario = false;
    this.inventarioEnEdicion = null;
  }

  alCompletarFormulario(): void {
    this.cerrarFormulario();
    this.cargar();
  }
}

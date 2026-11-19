import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { Proveedor } from '../../../core/models/proveedor.models';
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
  private readonly productoService = inject(ProductoService);

  readonly filtros = this.fb.nonNullable.group({
    busqueda: '',
    proveedorId: ''
  });

  productos: ProductoListItem[] = [];
  proveedores: Proveedor[] = [];
  pagina = 1;
  tamanoPagina = 20;
  total = 0;
  cargando = false;
  error = false;

  ngOnInit(): void {
    this.cargarProveedores();
    this.cargar();
  }

  buscar(): void {
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

  desactivar(producto: ProductoListItem): void {
    if (!window.confirm(`Desactivar el producto "${producto.nombre}"?`)) {
      return;
    }

    this.productoService.desactivar(producto.id).subscribe({
      next: () => this.cargar()
    });
  }

  get totalPaginas(): number {
    return Math.max(1, Math.ceil(this.total / this.tamanoPagina));
  }

  private cargarProveedores(): void {
    this.productoService.listarProveedores().subscribe({
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
}

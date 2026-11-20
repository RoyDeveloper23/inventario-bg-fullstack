import { Component, EventEmitter, inject, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';

import { Proveedor } from '../../../core/models/proveedor.models';
import { ProveedorService } from '../../../core/services/proveedor.service';
import { ProductoInventario } from '../models/inventario.models';
import { InventarioService } from '../services/inventario.service';

@Component({
  selector: 'app-inventory-form',
  imports: [ReactiveFormsModule, FormsModule],
  templateUrl: './inventory-form.component.html',
  styleUrl: './inventory-form.component.css'
})
export class InventoryFormComponent implements OnInit, OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly inventarioService = inject(InventarioService);
  private readonly proveedorService = inject(ProveedorService);

  @Input() productoId = '';
  @Input() inventario: ProductoInventario | null = null;
  @Output() completado = new EventEmitter<void>();
  @Output() cancelado = new EventEmitter<void>();

  readonly formulario = this.fb.nonNullable.group({
    proveedorId: ['', Validators.required],
    numeroLote: ['', [Validators.required, Validators.maxLength(100)]],
    precioUnitario: [0, [Validators.required, Validators.min(0.01)]],
    stockDisponible: [0, [Validators.required, Validators.min(1), Validators.pattern(/^\d+$/)]]
  });

  proveedores: Proveedor[] = [];
  cargandoProveedores = false;
  errorProveedores = false;
  enviando = false;
  eliminando = false;
  mostrandoNuevoProveedor = false;
  creandoProveedor = false;
  nombreNuevoProveedor = '';

  ngOnInit(): void {
    if (!this.inventario) {
      this.cargarProveedores();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['inventario'] && this.inventario) {
      this.formulario.controls.stockDisponible.setValidators([
        Validators.required,
        Validators.min(0),
        Validators.pattern(/^\d+$/)
      ]);
      this.formulario.controls.stockDisponible.updateValueAndValidity();
      this.formulario.patchValue({
        proveedorId: this.inventario.proveedorId,
        numeroLote: this.inventario.numeroLote,
        precioUnitario: this.inventario.precioUnitario,
        stockDisponible: this.inventario.stockDisponible
      });
    } else if (changes['inventario'] && !this.inventario) {
      this.formulario.controls.stockDisponible.setValidators([
        Validators.required,
        Validators.min(1),
        Validators.pattern(/^\d+$/)
      ]);
      this.formulario.controls.stockDisponible.updateValueAndValidity();
      this.formulario.reset({
        proveedorId: '',
        numeroLote: '',
        precioUnitario: 0,
        stockDisponible: 0
      });
    }
  }

  guardar(): void {
    if (this.formulario.invalid || !this.productoId || this.enviando) {
      this.formulario.markAllAsTouched();
      return;
    }

    const valores = this.formulario.getRawValue();
    const operacion = this.inventario
      ? this.inventarioService.actualizar(this.productoId, this.inventario.id, {
          precioUnitario: valores.precioUnitario,
          stockDisponible: valores.stockDisponible
        })
      : this.inventarioService.crear(this.productoId, {
          proveedorId: valores.proveedorId,
          numeroLote: valores.numeroLote.trim().toUpperCase(),
          precioUnitario: valores.precioUnitario,
          stockDisponible: valores.stockDisponible
        });

    this.enviando = true;
    operacion.pipe(finalize(() => this.enviando = false)).subscribe({
      next: () => this.completado.emit()
    });
  }

  eliminar(): void {
    if (!this.inventario || this.inventario.stockDisponible !== 0 || this.eliminando) {
      return;
    }
    if (!window.confirm(`¿Eliminar el lote "${this.inventario.numeroLote}"?`)) {
      return;
    }

    this.eliminando = true;
    this.inventarioService.eliminar(this.productoId, this.inventario.id)
      .pipe(finalize(() => this.eliminando = false))
      .subscribe({ next: () => this.completado.emit() });
  }

  cargarProveedores(): void {
    this.cargandoProveedores = true;
    this.errorProveedores = false;
    this.proveedorService.listar().pipe(
      finalize(() => this.cargandoProveedores = false)
    ).subscribe({
      next: (resultado) => this.proveedores = resultado.items,
      error: () => this.errorProveedores = true
    });
  }

  crearProveedor(): void {
    const nombre = this.nombreNuevoProveedor.trim();
    if (nombre.length < 2 || nombre.length > 200 || this.creandoProveedor) {
      return;
    }

    this.creandoProveedor = true;
    this.proveedorService.crear(nombre).pipe(
      finalize(() => this.creandoProveedor = false)
    ).subscribe({
      next: (proveedor) => {
        this.proveedores = [...this.proveedores, proveedor];
        this.formulario.controls.proveedorId.setValue(proveedor.id);
        this.nombreNuevoProveedor = '';
        this.mostrandoNuevoProveedor = false;
      }
    });
  }
}

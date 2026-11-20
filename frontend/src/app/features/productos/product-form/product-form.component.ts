import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { ApiError } from '../../../core/models/api-error.models';
import { ProductoRequest } from '../models/producto.models';
import { ProductoService } from '../services/producto.service';

@Component({
  selector: 'app-product-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.css'
})
export class ProductFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly productoService = inject(ProductoService);

  enviando = false;
  cargando = false;
  errorGuardar: string | null = null;
  id: string | null = null;

  readonly formulario = this.fb.nonNullable.group({
    codigoSku: ['', [Validators.required, Validators.maxLength(100)]],
    nombre: ['', [Validators.required, Validators.maxLength(200)]],
    descripcion: ['', [Validators.maxLength(1000)]],
    categoria: ['', [Validators.maxLength(100)]]
  });

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id');
    if (this.id) {
      this.cargar(this.id);
    }
  }

  guardar(): void {
    this.errorGuardar = null;
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    const datos: ProductoRequest = {
      ...this.formulario.getRawValue(),
      codigoSku: this.formulario.controls.codigoSku.value.trim().toUpperCase(),
      descripcion: this.formulario.controls.descripcion.value.trim() || null,
      categoria: this.formulario.controls.categoria.value.trim() || null
    };

    this.enviando = true;
    const operacion = this.id
      ? this.productoService.actualizar(this.id, datos)
      : this.productoService.crear(datos);

    operacion.pipe(
      finalize(() => this.enviando = false)
    ).subscribe({
      next: () => void this.router.navigate(['/productos']),
      error: (error: HttpErrorResponse) => {
        if (error.status === 409) {
          const problema = error.error as ApiError | undefined;
          this.errorGuardar = problema?.detail ?? 'Ocurrió un conflicto al guardar el producto.';
        }
      }
    });
  }

  private cargar(id: string): void {
    this.cargando = true;
    this.productoService.obtener(id).pipe(
      finalize(() => this.cargando = false)
    ).subscribe({
      next: (producto) => this.formulario.patchValue({
        codigoSku: producto.codigoSku,
        nombre: producto.nombre,
        descripcion: producto.descripcion ?? '',
        categoria: producto.categoria ?? ''
      })
    });
  }
}

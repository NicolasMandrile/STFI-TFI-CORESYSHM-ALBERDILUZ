export interface Categoria {
  id: number;
  nombre: string;
  descripcion?: string;
}

export interface CreateCategoria {
  nombre: string;
  descripcion?: string;
}

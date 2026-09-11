import type { Articulo } from "../domain/articulo";

export type ConsultarArticulosQuery = { busqueda?: string };

export type GuardarArticuloInput = {
  numero: string;
  descripcion: string;
};

export type ArticulosPort = {
  consultar(query: ConsultarArticulosQuery): Promise<Articulo[]>;
  consultarPorId(idArticulo: number): Promise<Articulo>;
  crear(input: GuardarArticuloInput): Promise<Articulo>;
  actualizar(
    idArticulo: number,
    input: GuardarArticuloInput,
  ): Promise<Articulo>;
  eliminar(idArticulo: number): Promise<void>;
};

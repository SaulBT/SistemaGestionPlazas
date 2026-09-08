import { z } from "zod";
import type { Articulo } from "../domain/articulo";

const articuloSchema = z.object({
  idArticulo: z.number().int().positive(),
  numero: z.string(),
  descripcion: z.string(),
});

export function toArticulo(dto: unknown): Articulo {
  return articuloSchema.parse(dto);
}

export function toArticulos(dto: unknown): Articulo[] {
  return z.array(articuloSchema).parse(dto);
}

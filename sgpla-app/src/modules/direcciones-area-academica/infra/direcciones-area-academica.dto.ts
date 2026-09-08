import { z } from "zod";
import type { Pagina } from "../application/direcciones-area-academica.contracts";
import type { DireccionAreaAcademica } from "../domain/direccion-area-academica";

const direccionAreaAcademicaDtoSchema = z.object({
  idAreaAcademica: z.number().int().positive(),
  nombre: z.string(),
  telefono: z.string(),
  extension: z.string(),
});

const paginaDtoSchema = z.object({
  items: z.array(z.unknown()),
  pagina: z.number().int().positive(),
  cantidad: z.number().int().positive(),
  total: z.number().int().nonnegative(),
});

export type PaginaDto<T> = {
  items: T[];
  pagina: number;
  cantidad: number;
  total: number;
};

export function toDireccionAreaAcademica(
  dto: unknown,
): DireccionAreaAcademica {
  return direccionAreaAcademicaDtoSchema.parse(dto);
}

export function toPagina<T>(
  dto: unknown,
  mapItem: (item: unknown) => T,
): Pagina<T> {
  const pagina = paginaDtoSchema.parse(dto);

  return {
    ...pagina,
    items: pagina.items.map(mapItem),
  };
}
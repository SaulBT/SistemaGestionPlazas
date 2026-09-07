import { z } from "zod";
import { REGIONES, type AreaAcademica, type EntidadAcademica } from "../domain/entidad-academica";
import type { Pagina } from "../application/entidades-academicas.contracts";

const entidadAcademicaDtoSchema = z.object({
  idEntidadAcademica: z.number().int().positive(),
  clave: z.string().nullable(),
  nombre: z.string(),
  calleNumero: z.string(),
  colonia: z.string(),
  cp: z.string(),
  municipio: z.string(),
  telefono: z.string(),
  extension: z.string(),
  idAreaAcademica: z.number().int().positive(),
  nombreAreaAcademica: z.string(),
  region: z.enum(REGIONES),
});

const areaAcademicaDtoSchema = z.object({
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

export type EntidadAcademicaDto = z.infer<typeof entidadAcademicaDtoSchema>;
export type AreaAcademicaDto = z.infer<typeof areaAcademicaDtoSchema>;
export type PaginaDto<T> = {
  items: T[];
  pagina: number;
  cantidad: number;
  total: number;
};

export function toEntidadAcademica(dto: unknown): EntidadAcademica {
  return entidadAcademicaDtoSchema.parse(dto);
}

export function toAreaAcademica(dto: unknown): AreaAcademica {
  return areaAcademicaDtoSchema.parse(dto);
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

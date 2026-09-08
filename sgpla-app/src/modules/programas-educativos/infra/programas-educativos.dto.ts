import { z } from "zod";
import type { Pagina } from "../application/programas-educativos.contracts";
import {
  REGIONES,
  type AreaAcademica,
  type EntidadAcademica,
  type PlanEstudio,
  type ProgramaEducativoDetalle,
  type ProgramaEducativo,
} from "../domain/programa-educativo";

const programaEducativoBaseDtoSchema = z.object({
  idProgramaEducativo: z.number().int().positive(),
  nombre: z.string(),
  campus: z.string(),
  idEntidadAcademica: z.number().int().positive(),
  nombreEntidadAcademica: z.string(),
  idAreaAcademica: z.number().int().positive(),
  nombreAreaAcademica: z.string(),
  region: z.enum(REGIONES),
});

const planesEstudioResumenSchema = z.array(
  z.object({
    idPlanEstudios: z.number().int().positive(),
    nombre: z.string(),
  }),
);

const programaEducativoDtoSchema = programaEducativoBaseDtoSchema.extend({
  planesEstudio: planesEstudioResumenSchema.default([]),
});

const programaEducativoDetalleDtoSchema = programaEducativoBaseDtoSchema.extend(
  {
    planesEstudio: z
      .array(
        z.object({
          idPlanEstudios: z.number().int().positive(),
          nombre: z.string(),
          modalidad: z.string().nullable(),
          idArchivo: z.number().int().positive().nullable(),
          nombreArchivo: z.string().nullable(),
          tipoArchivo: z.string().nullable(),
          tamanioArchivo: z.number().nullable(),
          cantidadExperienciasEducativas: z.number().int().nonnegative(),
        }),
      )
      .optional()
      .default([]),
  },
);

const areaAcademicaDtoSchema = z.object({
  idAreaAcademica: z.number().int().positive(),
  nombre: z.string(),
});

const entidadAcademicaDtoSchema = z.object({
  idEntidadAcademica: z.number().int().positive(),
  nombre: z.string(),
  idAreaAcademica: z.number().int().positive(),
  nombreAreaAcademica: z.string(),
  region: z.enum(REGIONES),
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

export function toProgramaEducativo(dto: unknown): ProgramaEducativo {
  return programaEducativoDtoSchema.parse(dto);
}

export function toProgramaEducativoDetalle(
  dto: unknown,
): ProgramaEducativoDetalle {
  return programaEducativoDetalleDtoSchema.parse(dto);
}

export function toAreaAcademica(dto: unknown): AreaAcademica {
  return areaAcademicaDtoSchema.parse(dto);
}

export function toEntidadAcademica(dto: unknown): EntidadAcademica {
  return entidadAcademicaDtoSchema.parse(dto);
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

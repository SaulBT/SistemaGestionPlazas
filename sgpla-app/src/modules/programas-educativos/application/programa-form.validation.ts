import { z } from "zod";
import { REGIONES } from "../domain/programa-educativo";
import type { ProgramaFormValues } from "./programa-form.model";

export const programaFormRequiredFields = [
  "clave",
  "nombre",
  "campus",
  "idEntidadAcademica",
  "idAreaAcademica",
  "region",
] as const satisfies readonly (keyof ProgramaFormValues)[];

export const programaFormSchema = z.object({
  clave: z
    .string()
    .trim()
    .min(1, "La clave es obligatoria.")
    .max(5, "La clave no puede superar los 5 caracteres."),
  nombre: z
    .string()
    .trim()
    .min(1, "El nombre del programa educativo es obligatorio.")
    .max(
      100,
      "El nombre del programa educativo no puede superar los 100 caracteres.",
    ),
  campus: z
    .string()
    .trim()
    .min(1, "El campus es obligatorio.")
    .max(100, "El campus no puede superar los 100 caracteres."),
  idEntidadAcademica: z
    .string()
    .min(1, "La entidad académica es obligatoria.")
    .refine(
      (value) => Number.isInteger(Number(value)) && Number(value) > 0,
      "Selecciona una entidad académica válida.",
    ),
  idAreaAcademica: z
    .string()
    .min(1, "El área académica es obligatoria.")
    .refine(
      (value) => Number.isInteger(Number(value)) && Number(value) > 0,
      "Selecciona un área académica válida.",
    ),
  region: z.enum(REGIONES, {
    error: "Selecciona una región válida.",
  }),
});

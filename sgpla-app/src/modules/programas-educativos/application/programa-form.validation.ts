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
  planesEstudio: z.array(
    z
      .object({
        idPlanEstudios: z.number().int().positive().optional(),
        nombre: z
          .string()
          .trim()
          .min(1, "El nombre del plan es obligatorio.")
          .max(100, "El nombre del plan no puede superar los 100 caracteres."),
        modalidad: z.string().trim(),
        archivo: z
          .custom<File>(
            (value) =>
              value === undefined ||
              (typeof File !== "undefined" && value instanceof File),
            "Selecciona un archivo válido.",
          )
          .optional(),
        nombreArchivo: z.string(),
        cantidadExperienciasEducativas: z
          .number()
          .int()
          .nonnegative()
          .optional(),
      })
      .superRefine((plan, context) => {
        if (plan.archivo && !/\.xlsx?$/i.test(plan.archivo.name)) {
          context.addIssue({
            code: "custom",
            path: ["archivo"],
            message: "Selecciona un archivo XLS o XLSX.",
          });
        }

        if (plan.archivo && plan.archivo.size > 10 * 1024 * 1024) {
          context.addIssue({
            code: "custom",
            path: ["archivo"],
            message: "El archivo del plan no puede superar 10 MB.",
          });
        }
      }),
  ),
});

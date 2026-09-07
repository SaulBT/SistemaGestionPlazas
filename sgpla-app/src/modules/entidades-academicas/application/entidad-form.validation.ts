import { z } from "zod";
import { REGIONES } from "../domain/entidad-academica";
import type { EntidadFormValues } from "./entidad-form.model";

export const entidadAcademicaRequiredFields = [
  "clave",
  "nombre",
  "calleNumero",
  "colonia",
  "cp",
  "municipio",
  "telefono",
  "extension",
  "idAreaAcademica",
  "region",
] as const satisfies readonly (keyof EntidadFormValues)[];

export const entidadAcademicaFormSchema = z.object({
  clave: z
    .string()
    .trim()
    .min(1, "La clave es obligatoria.")
    .regex(/^\d{5}$/, "La clave debe tener exactamente 5 dígitos."),
  nombre: z
    .string()
    .trim()
    .min(1, "El nombre es obligatorio.")
    .max(100, "El nombre no puede superar los 100 caracteres."),
  calleNumero: z
    .string()
    .trim()
    .min(1, "El domicilio es obligatorio.")
    .max(150, "El domicilio no puede superar los 150 caracteres."),
  colonia: z
    .string()
    .trim()
    .min(1, "La colonia es obligatoria.")
    .max(100, "La colonia no puede superar los 100 caracteres."),
  cp: z
    .string()
    .trim()
    .regex(/^\d{5}$/, "El C.P. debe tener exactamente 5 dígitos."),
  municipio: z
    .string()
    .trim()
    .min(1, "El municipio es obligatorio.")
    .max(100, "El municipio no puede superar los 100 caracteres."),
  telefono: z
    .string()
    .trim()
    .min(1, "El teléfono es obligatorio.")
    .max(30, "El teléfono no puede superar los 30 caracteres."),
  extension: z
    .string()
    .trim()
    .min(1, "La extensión es obligatoria.")
    .max(5, "La extensión no puede superar los 5 caracteres."),
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

export function toValidationErrors(error: z.ZodError) {
  return error.issues.reduce<Record<string, string[]>>((errors, issue) => {
    const field = issue.path[0]?.toString();

    if (!field) return errors;

    errors[field] ??= [];
    if (!errors[field].includes(issue.message)) {
      errors[field].push(issue.message);
    }

    return errors;
  }, {});
}

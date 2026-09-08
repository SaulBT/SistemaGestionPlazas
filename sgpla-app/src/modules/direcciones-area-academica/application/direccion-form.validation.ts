import { z } from "zod";
import type { DireccionFormValues } from "./direccion-form.model";

export const direccionFormRequiredFields = [
  "nombre",
  "telefono",
  "extension",
] as const satisfies readonly (keyof DireccionFormValues)[];

export const direccionFormSchema = z.object({
  nombre: z
    .string()
    .trim()
    .min(1, "El nombre es obligatorio.")
    .max(100, "El nombre no puede superar los 100 caracteres."),
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
});
import { z } from "zod";

export const articuloFormSchema = z.object({
  numero: z
    .string()
    .trim()
    .min(1, "El número del artículo es obligatorio.")
    .max(50, "El número no puede superar los 50 caracteres.")
    .refine(
      (value) => /\d/.test(value),
      "El número debe contener al menos un número.",
    ),
  descripcion: z
    .string()
    .trim()
    .min(1, "La descripción del artículo es obligatoria."),
});

export type ArticuloFormValues = z.infer<typeof articuloFormSchema>;

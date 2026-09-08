import type { DireccionAreaAcademica } from "../domain/direccion-area-academica";
import type { GuardarDireccionAreaAcademicaInput } from "./direcciones-area-academica.contracts";

export type DireccionFormValues = GuardarDireccionAreaAcademicaInput;

export function toDireccionFormValues(
  direccion?: DireccionAreaAcademica,
): DireccionFormValues {
  return {
    nombre: direccion?.nombre ?? "",
    telefono: direccion?.telefono ?? "",
    extension: direccion?.extension ?? "",
  };
}

export function toGuardarDireccionAreaAcademicaInput(
  values: DireccionFormValues,
): GuardarDireccionAreaAcademicaInput {
  return {
    nombre: values.nombre.trim(),
    telefono: values.telefono.trim(),
    extension: values.extension.trim(),
  };
}

export function hasDireccionFormChanges(
  values: DireccionFormValues,
  originalValues: DireccionFormValues,
) {
  return (Object.keys(values) as Array<keyof DireccionFormValues>).some(
    (field) => values[field].trim() !== originalValues[field].trim(),
  );
}
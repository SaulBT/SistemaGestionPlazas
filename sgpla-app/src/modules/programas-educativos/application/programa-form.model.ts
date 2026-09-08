import type { ProgramaEducativo } from "../domain/programa-educativo";
import type { GuardarProgramaEducativoInput } from "./programas-educativos.contracts";

export type ProgramaFormValues = {
  clave: string;
  nombre: string;
  campus: string;
  idEntidadAcademica: string;
  idAreaAcademica: string;
  region: string;
};

function separarClaveYNombre(nombreCompleto: string) {
  const separador = nombreCompleto.indexOf("-");

  return separador < 0
    ? { clave: "", nombre: nombreCompleto }
    : {
        clave: nombreCompleto.slice(0, separador).trim(),
        nombre: nombreCompleto.slice(separador + 1).trim(),
      };
}

export function toProgramaFormValues(
  programa?: ProgramaEducativo,
): ProgramaFormValues {
  const { clave, nombre } = separarClaveYNombre(programa?.nombre ?? "");

  return {
    clave,
    nombre,
    campus: programa?.campus ?? "",
    idEntidadAcademica: programa?.idEntidadAcademica.toString() ?? "",
    idAreaAcademica: programa?.idAreaAcademica.toString() ?? "",
    region: programa?.region ?? "",
  };
}

export function toGuardarProgramaEducativoInput(
  values: ProgramaFormValues,
): GuardarProgramaEducativoInput {
  return {
    nombre: `${values.clave.trim()}-${values.nombre.trim()}`,
    campus: values.campus.trim(),
    idEntidadAcademica: Number(values.idEntidadAcademica),
  };
}

export function hasProgramaFormChanges(
  values: ProgramaFormValues,
  originalValues: ProgramaFormValues,
) {
  return (Object.keys(values) as Array<keyof ProgramaFormValues>).some(
    (field) => values[field].trim() !== originalValues[field].trim(),
  );
}

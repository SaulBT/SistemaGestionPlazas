import type {
  EntidadAcademica,
} from "../domain/entidad-academica";
import type { GuardarEntidadAcademicaInput } from "./entidades-academicas.contracts";

export type EntidadFormValues = Omit<
  GuardarEntidadAcademicaInput,
  "idAreaAcademica" | "region"
> & {
  idAreaAcademica: string;
  region: string;
};

export function toEntidadFormValues(
  entidad?: EntidadAcademica,
): EntidadFormValues {
  return {
    clave: entidad?.clave ?? "",
    nombre: entidad?.nombre ?? "",
    calleNumero: entidad?.calleNumero ?? "",
    colonia: entidad?.colonia ?? "",
    cp: entidad?.cp ?? "",
    municipio: entidad?.municipio ?? "",
    telefono: entidad?.telefono ?? "",
    extension: entidad?.extension ?? "",
    idAreaAcademica: entidad?.idAreaAcademica.toString() ?? "",
    region: entidad?.region ?? "",
  };
}

export function toGuardarEntidadAcademicaInput(
  values: EntidadFormValues,
): GuardarEntidadAcademicaInput {
  return {
    ...values,
    clave: values.clave.trim(),
    nombre: values.nombre.trim(),
    calleNumero: values.calleNumero.trim(),
    colonia: values.colonia.trim(),
    cp: values.cp.trim(),
    municipio: values.municipio.trim(),
    telefono: values.telefono.trim(),
    extension: values.extension.trim(),
    idAreaAcademica: Number(values.idAreaAcademica),
    region: values.region.trim() as GuardarEntidadAcademicaInput["region"],
  };
}

export function hasEntidadFormChanges(
  values: EntidadFormValues,
  originalValues: EntidadFormValues,
) {
  return (Object.keys(values) as Array<keyof EntidadFormValues>).some(
    (field) => values[field].trim() !== originalValues[field].trim(),
  );
}

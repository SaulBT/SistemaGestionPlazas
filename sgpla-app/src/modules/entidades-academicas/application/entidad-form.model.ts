import type {
  EntidadAcademica,
  GuardarEntidadAcademicaInput,
} from "../domain/entidad-academica";

export type EntidadFormValues = Omit<
  GuardarEntidadAcademicaInput,
  "idAreaAcademica"
> & {
  idAreaAcademica: string;
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
    region: values.region.trim(),
  };
}

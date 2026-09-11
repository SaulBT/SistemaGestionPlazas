import { formatoTelefono } from "@/shared/formatters/telefono";
import type { EntidadAcademica } from "../domain/entidad-academica";

export type EntidadAcademicaTableRow = {
  id: string;
  entidad: EntidadAcademica;
  nombre: string;
  domicilio: string;
  telefono: string;
  nombreAreaAcademica: string;
  region: string;
};

export function toEntidadAcademicaTableRow(
  entidad: EntidadAcademica,
): EntidadAcademicaTableRow {
  return {
    id: entidad.idEntidadAcademica.toString(),
    entidad,
    nombre: entidad.nombre,
    domicilio: `${entidad.calleNumero}, ${entidad.colonia}, ${entidad.municipio}`,
    telefono: formatoTelefono(entidad.telefono),
    nombreAreaAcademica: entidad.nombreAreaAcademica,
    region: entidad.region,
  };
}

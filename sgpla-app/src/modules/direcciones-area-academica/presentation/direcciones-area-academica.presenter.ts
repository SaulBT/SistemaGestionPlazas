import type { DireccionAreaAcademica } from "../domain/direccion-area-academica";

export type DireccionAreaAcademicaTableRow = {
  id: string;
  direccion: DireccionAreaAcademica;
  nombre: string;
  telefono: string;
};

export function toDireccionAreaAcademicaTableRow(
  direccion: DireccionAreaAcademica,
): DireccionAreaAcademicaTableRow {
  return {
    id: direccion.idAreaAcademica.toString(),
    direccion,
    nombre: direccion.nombre,
    telefono: direccion.telefono,
  };
}
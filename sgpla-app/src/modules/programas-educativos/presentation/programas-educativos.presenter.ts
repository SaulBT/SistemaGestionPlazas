import type { ProgramaEducativo } from "../domain/programa-educativo";

export type ProgramaEducativoTableRow = {
  id: string;
  programa: ProgramaEducativo;
  nombre: string;
  region: string;
  nombreAreaAcademica: string;
  nombreEntidadAcademica: string;
  planesEstudio: ProgramaEducativo["planesEstudio"];
};

function obtenerNombreVisible(nombreCompleto: string) {
  const separador = nombreCompleto.indexOf("-");

  return separador < 0
    ? nombreCompleto
    : nombreCompleto.slice(separador + 1).trim();
}

export function toProgramaEducativoTableRow(
  programa: ProgramaEducativo,
): ProgramaEducativoTableRow {
  return {
    id: programa.idProgramaEducativo.toString(),
    programa,
    nombre: obtenerNombreVisible(programa.nombre),
    region: programa.region,
    nombreAreaAcademica: programa.nombreAreaAcademica,
    nombreEntidadAcademica: programa.nombreEntidadAcademica,
    planesEstudio: programa.planesEstudio,
  };
}

import type { EntidadAcademica } from "../domain/entidad-academica";
import type { EntidadesAcademicasPort } from "./entidades-academicas.port";
import type { GuardarEntidadAcademicaInput } from "./entidades-academicas.contracts";

export function guardarEntidadAcademica(
  port: EntidadesAcademicasPort,
  input: GuardarEntidadAcademicaInput,
  entidadExistente?: EntidadAcademica,
) {
  return entidadExistente
    ? port.actualizar(entidadExistente.idEntidadAcademica, input)
    : port.crear(input);
}

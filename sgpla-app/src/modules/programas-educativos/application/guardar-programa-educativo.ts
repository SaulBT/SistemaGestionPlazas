import type { ProgramaEducativo } from "../domain/programa-educativo";
import type { ProgramasEducativosPort } from "./programas-educativos.port";
import type { GuardarProgramaEducativoInput } from "./programas-educativos.contracts";

export function guardarProgramaEducativo(
  port: ProgramasEducativosPort,
  input: GuardarProgramaEducativoInput,
  programaExistente?: ProgramaEducativo,
) {
  return programaExistente
    ? port.actualizar(programaExistente.idProgramaEducativo, input)
    : port.crear(input);
}

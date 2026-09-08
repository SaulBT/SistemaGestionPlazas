import type {
  AreaAcademica,
  EntidadAcademica,
  ProgramaEducativo,
} from "../domain/programa-educativo";
import type {
  ConsultarProgramasEducativosQuery,
  GuardarProgramaEducativoInput,
  Pagina,
} from "./programas-educativos.contracts";

export interface ProgramasEducativosPort {
  consultar(
    query: ConsultarProgramasEducativosQuery,
  ): Promise<Pagina<ProgramaEducativo>>;
  consultarPorId(idProgramaEducativo: number): Promise<ProgramaEducativo>;
  crear(input: GuardarProgramaEducativoInput): Promise<ProgramaEducativo>;
  actualizar(
    idProgramaEducativo: number,
    input: GuardarProgramaEducativoInput,
  ): Promise<ProgramaEducativo>;
  eliminar(idProgramaEducativo: number): Promise<void>;
  consultarAreasAcademicas(): Promise<Pagina<AreaAcademica>>;
  consultarEntidadesAcademicas(): Promise<Pagina<EntidadAcademica>>;
}

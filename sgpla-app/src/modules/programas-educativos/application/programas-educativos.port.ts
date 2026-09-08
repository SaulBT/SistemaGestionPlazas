import type {
  AreaAcademica,
  EntidadAcademica,
  ProgramaEducativo,
  ProgramaEducativoDetalle,
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
  consultarPorId(
    idProgramaEducativo: number,
  ): Promise<ProgramaEducativoDetalle>;
  crear(
    input: GuardarProgramaEducativoInput,
  ): Promise<ProgramaEducativoDetalle>;
  actualizar(
    idProgramaEducativo: number,
    input: GuardarProgramaEducativoInput,
  ): Promise<ProgramaEducativoDetalle>;
  eliminar(idProgramaEducativo: number): Promise<void>;
  consultarAreasAcademicas(): Promise<Pagina<AreaAcademica>>;
  consultarEntidadesAcademicas(): Promise<Pagina<EntidadAcademica>>;
}

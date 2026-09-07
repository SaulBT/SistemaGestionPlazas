import type {
  EntidadAcademica,
  AreaAcademica,
} from "../domain/entidad-academica";
import type {
  ConsultarEntidadesAcademicasQuery,
  GuardarEntidadAcademicaInput,
  Pagina,
} from "./entidades-academicas.contracts";

export interface EntidadesAcademicasPort {
  consultar(
    query: ConsultarEntidadesAcademicasQuery,
  ): Promise<Pagina<EntidadAcademica>>;
  consultarPorId(idEntidadAcademica: number): Promise<EntidadAcademica>;
  crear(input: GuardarEntidadAcademicaInput): Promise<EntidadAcademica>;
  actualizar(
    idEntidadAcademica: number,
    input: GuardarEntidadAcademicaInput,
  ): Promise<EntidadAcademica>;
  eliminar(idEntidadAcademica: number): Promise<void>;
  consultarAreasAcademicas(): Promise<Pagina<AreaAcademica>>;
}

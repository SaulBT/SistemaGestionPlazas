import type {
  AreaAcademica,
  ConsultarEntidadesAcademicasQuery,
  EntidadAcademica,
  GuardarEntidadAcademicaInput,
  Pagina,
} from "../domain/entidad-academica";

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

import type { DireccionAreaAcademica } from "../domain/direccion-area-academica";
import type {
  ConsultarDireccionesAreaAcademicaQuery,
  GuardarDireccionAreaAcademicaInput,
  Pagina,
} from "./direcciones-area-academica.contracts";

export interface DireccionesAreaAcademicaPort {
  consultar(
    query: ConsultarDireccionesAreaAcademicaQuery,
  ): Promise<Pagina<DireccionAreaAcademica>>;
  consultarPorId(idAreaAcademica: number): Promise<DireccionAreaAcademica>;
  crear(
    input: GuardarDireccionAreaAcademicaInput,
  ): Promise<DireccionAreaAcademica>;
  actualizar(
    idAreaAcademica: number,
    input: GuardarDireccionAreaAcademicaInput,
  ): Promise<DireccionAreaAcademica>;
  eliminar(idAreaAcademica: number): Promise<void>;
}
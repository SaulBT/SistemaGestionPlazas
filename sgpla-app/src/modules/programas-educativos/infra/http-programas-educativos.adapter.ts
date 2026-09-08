import { httpClient, withQuery } from "@/shared/api/http-client";
import type { ProgramasEducativosPort } from "../application/programas-educativos.port";
import type {
  ConsultarProgramasEducativosQuery,
  GuardarProgramaEducativoInput,
} from "../application/programas-educativos.contracts";
import {
  toAreaAcademica,
  toEntidadAcademica,
  toPagina,
  toProgramaEducativo,
  type PaginaDto,
} from "./programas-educativos.dto";

const programasEducativosPath = "/api/v1/programas-educativos";
const areasAcademicasPath = "/api/v1/areas-academicas";
const entidadesAcademicasPath = "/api/v1/entidades-academicas";

export const httpProgramasEducativosAdapter: ProgramasEducativosPort = {
  async consultar(query: ConsultarProgramasEducativosQuery) {
    const response = await httpClient.get<PaginaDto<unknown>>(
      withQuery(programasEducativosPath, query),
    );

    return toPagina(response, toProgramaEducativo);
  },

  async consultarPorId(idProgramaEducativo: number) {
    const response = await httpClient.get<unknown>(
      `${programasEducativosPath}/${idProgramaEducativo}`,
    );

    return toProgramaEducativo(response);
  },

  async crear(input: GuardarProgramaEducativoInput) {
    const response = await httpClient.post<unknown>(
      programasEducativosPath,
      input,
    );

    return toProgramaEducativo(response);
  },

  async actualizar(
    idProgramaEducativo: number,
    input: GuardarProgramaEducativoInput,
  ) {
    const response = await httpClient.put<unknown>(
      `${programasEducativosPath}/${idProgramaEducativo}`,
      input,
    );

    return toProgramaEducativo(response);
  },

  eliminar(idProgramaEducativo: number) {
    return httpClient.delete(
      `${programasEducativosPath}/${idProgramaEducativo}`,
    );
  },

  async consultarAreasAcademicas() {
    const response = await httpClient.get<PaginaDto<unknown>>(
      withQuery(areasAcademicasPath, { pagina: 1, cantidad: 100 }),
    );

    return toPagina(response, toAreaAcademica);
  },

  async consultarEntidadesAcademicas() {
    const response = await httpClient.get<PaginaDto<unknown>>(
      withQuery(entidadesAcademicasPath, { pagina: 1, cantidad: 100 }),
    );

    return toPagina(response, toEntidadAcademica);
  },
};

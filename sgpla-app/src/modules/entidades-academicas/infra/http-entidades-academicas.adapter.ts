import { httpClient, withQuery } from "@/shared/api/http-client";
import type { EntidadesAcademicasPort } from "../application/entidades-academicas.port";
import type {
  ConsultarEntidadesAcademicasQuery,
  GuardarEntidadAcademicaInput,
} from "../application/entidades-academicas.contracts";
import {
  toAreaAcademica,
  toEntidadAcademica,
  toPagina,
  type PaginaDto,
} from "./entidades-academicas.dto";

const entidadesAcademicasPath = "/api/v1/entidades-academicas";
const areasAcademicasPath = "/api/v1/areas-academicas";

export const httpEntidadesAcademicasAdapter: EntidadesAcademicasPort = {
  async consultar(query: ConsultarEntidadesAcademicasQuery) {
    const response = await httpClient.get<PaginaDto<unknown>>(
      withQuery(entidadesAcademicasPath, query),
    );

    return toPagina(response, toEntidadAcademica);
  },

  async consultarPorId(idEntidadAcademica: number) {
    const response = await httpClient.get<unknown>(
      `${entidadesAcademicasPath}/${idEntidadAcademica}`,
    );

    return toEntidadAcademica(response);
  },

  async crear(input: GuardarEntidadAcademicaInput) {
    const response = await httpClient.post<unknown>(
      entidadesAcademicasPath,
      input,
    );

    return toEntidadAcademica(response);
  },

  async actualizar(
    idEntidadAcademica: number,
    input: GuardarEntidadAcademicaInput,
  ) {
    const response = await httpClient.put<unknown>(
      `${entidadesAcademicasPath}/${idEntidadAcademica}`,
      input,
    );

    return toEntidadAcademica(response);
  },

  eliminar(idEntidadAcademica: number) {
    return httpClient.delete(`${entidadesAcademicasPath}/${idEntidadAcademica}`);
  },

  async consultarAreasAcademicas() {
    const response = await httpClient.get<PaginaDto<unknown>>(
      withQuery(areasAcademicasPath, { pagina: 1, cantidad: 100 }),
    );

    return toPagina(response, toAreaAcademica);
  },
};

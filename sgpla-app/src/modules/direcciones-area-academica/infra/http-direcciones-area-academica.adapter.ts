import { httpClient, withQuery } from "@/shared/api/http-client";
import type { DireccionesAreaAcademicaPort } from "../application/direcciones-area-academica.port";
import type {
  ConsultarDireccionesAreaAcademicaQuery,
  GuardarDireccionAreaAcademicaInput,
} from "../application/direcciones-area-academica.contracts";
import {
  toDireccionAreaAcademica,
  toPagina,
  type PaginaDto,
} from "./direcciones-area-academica.dto";

const areasAcademicasPath = "/api/v1/areas-academicas";

export const httpDireccionesAreaAcademicaAdapter: DireccionesAreaAcademicaPort = {
  async consultar(query: ConsultarDireccionesAreaAcademicaQuery) {
    const response = await httpClient.get<PaginaDto<unknown>>(
      withQuery(areasAcademicasPath, query),
    );

    return toPagina(response, toDireccionAreaAcademica);
  },

  async consultarPorId(idAreaAcademica: number) {
    const response = await httpClient.get<unknown>(
      `${areasAcademicasPath}/${idAreaAcademica}`,
    );

    return toDireccionAreaAcademica(response);
  },

  async crear(input: GuardarDireccionAreaAcademicaInput) {
    const response = await httpClient.post<unknown>(areasAcademicasPath, input);

    return toDireccionAreaAcademica(response);
  },

  async actualizar(
    idAreaAcademica: number,
    input: GuardarDireccionAreaAcademicaInput,
  ) {
    const response = await httpClient.put<unknown>(
      `${areasAcademicasPath}/${idAreaAcademica}`,
      input,
    );

    return toDireccionAreaAcademica(response);
  },

  eliminar(idAreaAcademica: number) {
    return httpClient.delete(`${areasAcademicasPath}/${idAreaAcademica}`);
  },
};
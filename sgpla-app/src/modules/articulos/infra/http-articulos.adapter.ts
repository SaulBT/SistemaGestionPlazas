import { httpClient, withQuery } from "@/shared/api/http-client";
import type {
  ArticulosPort,
  ConsultarArticulosQuery,
  GuardarArticuloInput,
} from "../application/articulos.contracts";
import { toArticulo, toArticulos } from "./articulos.dto";

const articulosPath = "/api/v1/articulos";

export const httpArticulosAdapter: ArticulosPort = {
  async consultar(query: ConsultarArticulosQuery) {
    const response = await httpClient.get<unknown>(
      withQuery(articulosPath, query),
    );
    return toArticulos(response);
  },
  async consultarPorId(idArticulo) {
    return toArticulo(
      await httpClient.get<unknown>(`${articulosPath}/${idArticulo}`),
    );
  },
  async crear(input: GuardarArticuloInput) {
    return toArticulo(await httpClient.post<unknown>(articulosPath, input));
  },
  async actualizar(idArticulo, input) {
    return toArticulo(
      await httpClient.put<unknown>(`${articulosPath}/${idArticulo}`, input),
    );
  },
  eliminar(idArticulo) {
    return httpClient.delete(`${articulosPath}/${idArticulo}`);
  },
};

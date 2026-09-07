import { httpClient, withQuery } from "@/shared/api/http-client";
import type { EntidadesAcademicasPort } from "../application/entidades-academicas.port";
import type {
  AreaAcademica,
  ConsultarEntidadesAcademicasQuery,
  EntidadAcademica,
  GuardarEntidadAcademicaInput,
  Pagina,
} from "../domain/entidad-academica";

const entidadesAcademicasPath = "/api/v1/entidades-academicas";
const areasAcademicasPath = "/api/v1/areas-academicas";

type EntidadAcademicaDto = EntidadAcademica;
type AreaAcademicaDto = AreaAcademica;

type PaginaDto<T> = {
  items: T[];
  pagina: number;
  cantidad: number;
  total: number;
};

function mapEntidadAcademica(dto: EntidadAcademicaDto): EntidadAcademica {
  return { ...dto };
}

function mapAreaAcademica(dto: AreaAcademicaDto): AreaAcademica {
  return { ...dto };
}

function mapPagina<TDto, T>(
  dto: PaginaDto<TDto>,
  mapItem: (item: TDto) => T,
): Pagina<T> {
  return {
    items: dto.items.map(mapItem),
    pagina: dto.pagina,
    cantidad: dto.cantidad,
    total: dto.total,
  };
}

export const httpEntidadesAcademicasAdapter: EntidadesAcademicasPort = {
  async consultar(query: ConsultarEntidadesAcademicasQuery) {
    const response = await httpClient.get<PaginaDto<EntidadAcademicaDto>>(
      withQuery(entidadesAcademicasPath, query),
    );

    return mapPagina(response, mapEntidadAcademica);
  },

  async consultarPorId(idEntidadAcademica: number) {
    const response = await httpClient.get<EntidadAcademicaDto>(
      `${entidadesAcademicasPath}/${idEntidadAcademica}`,
    );

    return mapEntidadAcademica(response);
  },

  async crear(input: GuardarEntidadAcademicaInput) {
    const response = await httpClient.post<EntidadAcademicaDto>(
      entidadesAcademicasPath,
      input,
    );

    return mapEntidadAcademica(response);
  },

  async actualizar(
    idEntidadAcademica: number,
    input: GuardarEntidadAcademicaInput,
  ) {
    const response = await httpClient.put<EntidadAcademicaDto>(
      `${entidadesAcademicasPath}/${idEntidadAcademica}`,
      input,
    );

    return mapEntidadAcademica(response);
  },

  eliminar(idEntidadAcademica: number) {
    return httpClient.delete(`${entidadesAcademicasPath}/${idEntidadAcademica}`);
  },

  async consultarAreasAcademicas() {
    const response = await httpClient.get<PaginaDto<AreaAcademicaDto>>(
      withQuery(areasAcademicasPath, { pagina: 1, cantidad: 100 }),
    );

    return mapPagina(response, mapAreaAcademica);
  },
};

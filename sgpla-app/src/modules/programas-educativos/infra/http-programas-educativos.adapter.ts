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
  toProgramaEducativoDetalle,
  type PaginaDto,
} from "./programas-educativos.dto";

const programasEducativosPath = "/api/v1/programas-educativos";
const areasAcademicasPath = "/api/v1/areas-academicas";
const entidadesAcademicasPath = "/api/v1/entidades-academicas";

function toProgramaEducativoFormData(input: GuardarProgramaEducativoInput) {
  const formData = new FormData();
  formData.set("nombre", input.nombre);
  formData.set("campus", input.campus);
  formData.set("idEntidadAcademica", String(input.idEntidadAcademica));

  input.planesEstudio.forEach((plan, index) => {
    const prefix = `planesEstudio[${index}]`;
    if (plan.idPlanEstudios) {
      formData.set(`${prefix}.idPlanEstudios`, String(plan.idPlanEstudios));
    }

    formData.set(`${prefix}.nombre`, plan.nombre);
    formData.set(`${prefix}.modalidad`, plan.modalidad);
    if (plan.archivo) {
      formData.set(`${prefix}.archivo`, plan.archivo, plan.archivo.name);
    }
  });

  return formData;
}

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

    return toProgramaEducativoDetalle(response);
  },

  async crear(input: GuardarProgramaEducativoInput) {
    const response = await httpClient.post<unknown>(
      programasEducativosPath,
      toProgramaEducativoFormData(input),
    );

    return toProgramaEducativoDetalle(response);
  },

  async actualizar(
    idProgramaEducativo: number,
    input: GuardarProgramaEducativoInput,
  ) {
    const response = await httpClient.put<unknown>(
      `${programasEducativosPath}/${idProgramaEducativo}`,
      toProgramaEducativoFormData(input),
    );

    return toProgramaEducativoDetalle(response);
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

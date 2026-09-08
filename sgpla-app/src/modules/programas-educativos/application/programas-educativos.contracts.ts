import type { Region } from "../domain/programa-educativo";

export type Pagina<T> = {
  items: T[];
  pagina: number;
  cantidad: number;
  total: number;
};

export type ConsultarProgramasEducativosQuery = {
  busqueda?: string;
  region?: Region;
  idAreaAcademica?: number;
  idEntidadAcademica?: number;
  pagina: number;
  cantidad: number;
};

export type GuardarProgramaEducativoInput = {
  nombre: string;
  campus: string;
  idEntidadAcademica: number;
  planesEstudio: PlanEstudioInput[];
};

export type PlanEstudioInput = {
  idPlanEstudios?: number;
  nombre: string;
  modalidad: string;
  archivo?: File;
};

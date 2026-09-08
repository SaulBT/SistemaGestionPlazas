export const REGIONES = [
  "1-Xalapa",
  "2-Veracruz",
  "3-Orizaba-Córdoba",
  "4-Poza Rica-Túxpan",
  "5-Coatzacoalcos-Minatitlán",
] as const;

export type Region = (typeof REGIONES)[number];

export type ProgramaEducativo = {
  idProgramaEducativo: number;
  nombre: string;
  campus: string;
  idEntidadAcademica: number;
  nombreEntidadAcademica: string;
  idAreaAcademica: number;
  nombreAreaAcademica: string;
  region: Region;
  planesEstudio: PlanEstudioResumen[];
};

export type ProgramaEducativoDetalle = Omit<
  ProgramaEducativo,
  "planesEstudio"
> & {
  planesEstudio: PlanEstudio[];
};

export type PlanEstudioResumen = {
  idPlanEstudios: number;
  nombre: string;
};

export type PlanEstudio = {
  idPlanEstudios: number;
  nombre: string;
  modalidad: string | null;
  idArchivo: number | null;
  nombreArchivo: string | null;
  tipoArchivo: string | null;
  tamanioArchivo: number | null;
  cantidadExperienciasEducativas: number;
};

export const MODALIDADES_PLAN_ESTUDIOS = [
  "Escolarizado",
  "Abierto",
  "Virtual",
  "Mixta",
  "Semi escolarizado",
  "A distancia",
] as const;

export type ModalidadPlanEstudios = (typeof MODALIDADES_PLAN_ESTUDIOS)[number];

export type AreaAcademica = {
  idAreaAcademica: number;
  nombre: string;
};

export type EntidadAcademica = {
  idEntidadAcademica: number;
  nombre: string;
  idAreaAcademica: number;
  nombreAreaAcademica: string;
  region: Region;
};

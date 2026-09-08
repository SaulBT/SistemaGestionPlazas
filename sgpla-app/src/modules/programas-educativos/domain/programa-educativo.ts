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
};

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

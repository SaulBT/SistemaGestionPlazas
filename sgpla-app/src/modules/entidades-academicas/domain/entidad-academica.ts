export const REGIONES = [
  "1-Xalapa",
  "2-Veracruz",
  "3-Orizaba-Córdoba",
  "4-Poza Rica-Túxpan",
  "5-Coatzacoalcos-Minatitlán",
] as const;

export type Region = (typeof REGIONES)[number];

export type EntidadAcademica = {
  idEntidadAcademica: number;
  clave: string | null;
  nombre: string;
  calleNumero: string;
  colonia: string;
  cp: string;
  municipio: string;
  telefono: string;
  extension: string;
  idAreaAcademica: number;
  nombreAreaAcademica: string;
  region: Region;
};

export type AreaAcademica = {
  idAreaAcademica: number;
  nombre: string;
  telefono: string;
  extension: string;
};

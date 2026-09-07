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
  region: string;
};

export type Pagina<T> = {
  items: T[];
  pagina: number;
  cantidad: number;
  total: number;
};

export type ConsultarEntidadesAcademicasQuery = {
  busqueda?: string;
  region?: string;
  idAreaAcademica?: number;
  pagina: number;
  cantidad: number;
};

export type GuardarEntidadAcademicaInput = {
  clave: string;
  nombre: string;
  calleNumero: string;
  colonia: string;
  cp: string;
  municipio: string;
  telefono: string;
  extension: string;
  idAreaAcademica: number;
  region: string;
};

export type AreaAcademica = {
  idAreaAcademica: number;
  nombre: string;
  telefono: string;
  extension: string;
};

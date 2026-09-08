import type { Region } from "../domain/entidad-academica";

export type Pagina<T> = {
  items: T[];
  pagina: number;
  cantidad: number;
  total: number;
};

export type ConsultarEntidadesAcademicasQuery = {
  busqueda?: string;
  region?: Region;
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
  region: Region;
};

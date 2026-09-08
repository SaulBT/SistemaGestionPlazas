export type Pagina<T> = {
  items: T[];
  pagina: number;
  cantidad: number;
  total: number;
};

export type ConsultarDireccionesAreaAcademicaQuery = {
  busqueda?: string;
  pagina: number;
  cantidad: number;
};

export type GuardarDireccionAreaAcademicaInput = {
  nombre: string;
  telefono: string;
  extension: string;
};
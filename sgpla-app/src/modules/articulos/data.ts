export type ArticuloListItem = {
  id: string;
  numero: string;
  descripcion: string;
};

export const articulos: ArticuloListItem[] = [
  {
    id: "1",
    numero: "Artículo 1",
    descripcion: "Disposiciones generales para la gestión de plazas vacantes.",
  },
  {
    id: "2",
    numero: "Artículo 2",
    descripcion: "Definiciones y criterios aplicables al sistema.",
  },
  {
    id: "3",
    numero: "Artículo 3",
    descripcion: "Responsabilidades de las entidades académicas.",
  },
  {
    id: "4",
    numero: "Artículo 4",
    descripcion: "Procedimiento para el registro de información.",
  },
  {
    id: "5",
    numero: "Artículo 5",
    descripcion: "Disposiciones para la actualización de datos.",
  },
];

export type PeriodoEscolarListItem = {
  id: string;
  codigo: string;
  anio: string;
  periodo: "Febrero-Julio" | "Agosto-Enero";
  periodoMostrar: string;
};

export const periodosEscolares: PeriodoEscolarListItem[] = [
  {
    id: "1",
    codigo: "202551",
    anio: "2025",
    periodo: "Febrero-Julio",
    periodoMostrar: "Febrero – Julio 2025",
  },
  {
    id: "2",
    codigo: "202501",
    anio: "2025",
    periodo: "Agosto-Enero",
    periodoMostrar: "Agosto 2024 – Enero 2025",
  },
  {
    id: "3",
    codigo: "202451",
    anio: "2024",
    periodo: "Febrero-Julio",
    periodoMostrar: "Febrero – Julio 2024",
  },
  {
    id: "4",
    codigo: "202401",
    anio: "2024",
    periodo: "Agosto-Enero",
    periodoMostrar: "Agosto 2023 – Enero 2024",
  },
];

export const periodosOptions = ["Febrero-Julio", "Agosto-Enero"] as const;

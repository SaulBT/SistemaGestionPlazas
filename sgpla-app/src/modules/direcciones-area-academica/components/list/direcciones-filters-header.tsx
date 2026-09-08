"use client";

import type { ConsultarDireccionesAreaAcademicaQuery } from "../../application/direcciones-area-academica.contracts";
import { DireccionAreaAcademicaSearchInput } from "./ui/direccion-area-academica-search-input";

type Props = {
  consulta: ConsultarDireccionesAreaAcademicaQuery;
  onChange: (cambios: Partial<ConsultarDireccionesAreaAcademicaQuery>) => void;
};

export function DireccionesFiltersHeader({ consulta, onChange }: Props) {
  return (
    <div className="flex flex-col items-end gap-3 sm:flex-row sm:justify-between">
      <DireccionAreaAcademicaSearchInput
        value={consulta.busqueda ?? ""}
        onChange={(busqueda) => onChange({ busqueda })}
      />
    </div>
  );
}
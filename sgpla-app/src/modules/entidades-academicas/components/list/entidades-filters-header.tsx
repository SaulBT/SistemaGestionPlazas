"use client";

import { useCallback } from "react";
import { Field, FieldLabel } from "@/components/ui/field";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  REGIONES,
  type ConsultarEntidadesAcademicasQuery,
} from "../../domain/entidad-academica";
import { useAreasAcademicas } from "../../presentation/entidades-academicas.queries";
import { EntidadAcademicaSearchInput } from "./ui/entidad-academica-search-input";

const TODAS = "__todas__";

type Props = {
  consulta: ConsultarEntidadesAcademicasQuery;
  onChange: (cambios: Partial<ConsultarEntidadesAcademicasQuery>) => void;
};

export function EntidadesFiltersHeader({ consulta, onChange }: Props) {
  const areasQuery = useAreasAcademicas();
  const actualizarBusqueda = useCallback(
    (busqueda: string) => onChange({ busqueda }),
    [onChange],
  );

  return (
    <div className="flex flex-col items-end gap-3 sm:flex-row sm:justify-between">
      <EntidadAcademicaSearchInput
        value={consulta.busqueda ?? ""}
        onChange={actualizarBusqueda}
      />

      <div className="flex w-full flex-col gap-3 sm:w-auto sm:flex-row">
        <Field className="w-full shrink-0 sm:w-48">
          <FieldLabel htmlFor="region">Región</FieldLabel>
          <Select
            value={consulta.region ?? TODAS}
            onValueChange={(value) =>
              onChange({ region: value === TODAS ? undefined : value })
            }
          >
            <SelectTrigger id="region" className="w-full">
              <SelectValue placeholder="Todas las regiones" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value={TODAS}>Todas las regiones</SelectItem>
              {REGIONES.map((region) => (
                <SelectItem key={region} value={region}>
                  {region}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </Field>

        <Field className="w-full shrink-0 sm:w-56">
          <FieldLabel htmlFor="idAreaAcademica">Área Académica</FieldLabel>
          <Select
            value={consulta.idAreaAcademica?.toString() ?? TODAS}
            onValueChange={(value) =>
              onChange({
                idAreaAcademica:
                  value === TODAS ? undefined : Number(value),
              })
            }
            disabled={areasQuery.isPending}
          >
            <SelectTrigger id="idAreaAcademica" className="w-full">
              <SelectValue placeholder="Todas las áreas" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value={TODAS}>Todas las áreas</SelectItem>
              {(areasQuery.data?.items ?? []).map((area) => (
                <SelectItem
                  key={area.idAreaAcademica}
                  value={area.idAreaAcademica.toString()}
                >
                  {area.nombre}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </Field>
      </div>
    </div>
  );
}

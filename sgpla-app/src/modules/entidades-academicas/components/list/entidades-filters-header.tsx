"use client";

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
  type Region,
} from "../../domain/entidad-academica";
import type { ConsultarEntidadesAcademicasQuery } from "../../application/entidades-academicas.contracts";
import { EntidadAcademicaSearchInput } from "./ui/entidad-academica-search-input";

const TODAS = "__todas__";

type Props = {
  consulta: ConsultarEntidadesAcademicasQuery;
  areas: Array<{ value: string; label: string }>;
  areasLoading: boolean;
  onChange: (cambios: Partial<ConsultarEntidadesAcademicasQuery>) => void;
};

export function EntidadesFiltersHeader({
  consulta,
  areas,
  areasLoading,
  onChange,
}: Props) {

  return (
    <div className="flex flex-col items-end gap-3 sm:flex-row sm:justify-between">
      <EntidadAcademicaSearchInput
        value={consulta.busqueda ?? ""}
        onChange={(busqueda) => onChange({ busqueda })}
      />

      <div className="flex w-full flex-col gap-3 sm:w-auto sm:flex-row">
        <Field className="w-full shrink-0 sm:w-48">
          <FieldLabel htmlFor="region">Región</FieldLabel>
          <Select
            value={consulta.region ?? TODAS}
            onValueChange={(value) =>
              onChange({
                region: value === TODAS ? undefined : (value as Region),
              })
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
            disabled={areasLoading}
          >
            <SelectTrigger id="idAreaAcademica" className="w-full">
              <SelectValue placeholder="Todas las áreas" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value={TODAS}>Todas las áreas</SelectItem>
              {areas.map((area) => (
                <SelectItem
                  key={area.value}
                  value={area.value}
                >
                  {area.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </Field>
      </div>
    </div>
  );
}

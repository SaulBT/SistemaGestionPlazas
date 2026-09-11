"use client";
import { Field, FieldLabel } from "@/components/ui/field";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { ConsultarProgramasEducativosQuery } from "../../application/programas-educativos.contracts";
import { REGIONES, type Region } from "../../domain/programa-educativo";
import { ProgramaEducativoSearchInput } from "./ui/programa-educativo-search-input";
const TODAS = "__todas__";
type Props = {
  consulta: ConsultarProgramasEducativosQuery;
  areas: Array<{ value: string; label: string }>;
  areasLoading: boolean;
  entidades: Array<{ value: string; label: string }>;
  entidadesLoading: boolean;
  onChange: (cambios: Partial<ConsultarProgramasEducativosQuery>) => void;
};
export function ProgramasFiltersHeader({
  consulta,
  areas,
  areasLoading,
  entidades,
  entidadesLoading,
  onChange,
}: Props) {
  return (
    <div className="flex flex-col items-end gap-3 sm:flex-row sm:justify-between">
      <ProgramaEducativoSearchInput
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
        <Field className="w-full shrink-0 sm:w-48">
          <FieldLabel htmlFor="idAreaAcademica">Área Académica</FieldLabel>
          <Select
            value={consulta.idAreaAcademica?.toString() ?? TODAS}
            onValueChange={(value) =>
              onChange({
                idAreaAcademica: value === TODAS ? undefined : Number(value),
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
                <SelectItem key={area.value} value={area.value}>
                  {area.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </Field>
        <Field className="w-full shrink-0 sm:w-56">
          <FieldLabel htmlFor="idEntidadAcademica">
            Entidad Académica
          </FieldLabel>
          <Select
            value={consulta.idEntidadAcademica?.toString() ?? TODAS}
            onValueChange={(value) =>
              onChange({
                idEntidadAcademica: value === TODAS ? undefined : Number(value),
              })
            }
            disabled={entidadesLoading}
          >
            <SelectTrigger id="idEntidadAcademica" className="w-full">
              <SelectValue placeholder="Todas las entidades" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value={TODAS}>Todas las entidades</SelectItem>
              {entidades.map((entidad) => (
                <SelectItem key={entidad.value} value={entidad.value}>
                  {entidad.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </Field>
      </div>
    </div>
  );
}

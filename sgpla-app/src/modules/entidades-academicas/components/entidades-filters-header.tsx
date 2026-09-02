"use client";

import { Search } from "lucide-react";
import { Field, FieldLabel } from "@/components/ui/field";
import { InputGroup, InputGroupAddon, InputGroupInput } from "@/components/ui/input-group";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { areasAcademicas, regiones } from "../data";

function FilterSelect({ id, label, name, options }: { id: string; label: string; name: string; options: string[] }) {
  return (
    <Field>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <Select name={name}>
        <SelectTrigger id={id} className="w-full"><SelectValue placeholder={`Todas las ${label.toLowerCase()}`} /></SelectTrigger>
        <SelectContent>{options.map((option) => <SelectItem key={option} value={option}>{option}</SelectItem>)}</SelectContent>
      </Select>
    </Field>
  );
}

export function EntidadesFiltersHeader() {
  return (
    <div className="flex flex-col items-end gap-3 sm:flex-row sm:justify-between">
      <Field className="w-full sm:max-w-xs">
        <FieldLabel htmlFor="busqueda">Buscar entidad académica</FieldLabel>
        <InputGroup>
          <InputGroupInput id="busqueda" name="busqueda" placeholder="Buscar entidad académica..." />
          <InputGroupAddon><Search className="size-3.5" /></InputGroupAddon>
        </InputGroup>
      </Field>
      <div className="flex w-full flex-col gap-3 sm:w-auto sm:flex-row">
        <div className="w-full shrink-0 sm:w-48"><FilterSelect id="region" label="Región" name="region" options={regiones} /></div>
        <div className="w-full shrink-0 sm:w-48"><FilterSelect id="idAreaAcademica" label="Área Académica" name="idAreaAcademica" options={areasAcademicas} /></div>
      </div>
    </div>
  );
}

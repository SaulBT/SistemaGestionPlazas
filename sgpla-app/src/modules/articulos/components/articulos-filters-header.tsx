"use client";

import { Search } from "lucide-react";
import { Field, FieldLabel } from "@/components/ui/field";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from "@/components/ui/input-group";

type Props = { value: string; onChange: (value: string) => void };

export function ArticulosFiltersHeader({ value, onChange }: Props) {
  return (
    <div className="flex flex-col items-end gap-3 sm:flex-row sm:justify-between">
      <Field className="w-full sm:max-w-xs">
        <FieldLabel htmlFor="busqueda-articulos">Buscar artículo</FieldLabel>
        <InputGroup>
          <InputGroupInput
            id="busqueda-articulos"
            value={value}
            onChange={(event) => onChange(event.target.value)}
            placeholder="Buscar por número o descripción..."
          />
          <InputGroupAddon>
            <Search className="size-3.5" />
          </InputGroupAddon>
        </InputGroup>
      </Field>
    </div>
  );
}

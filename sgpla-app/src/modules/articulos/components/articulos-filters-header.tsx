"use client";

import { Search } from "lucide-react";

import { Field, FieldLabel } from "@/components/ui/field";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from "@/components/ui/input-group";

export function ArticulosFiltersHeader() {
  return (
    <div className="flex flex-col items-end gap-3 sm:flex-row sm:justify-between">
      <Field className="w-full sm:max-w-xs">
        <FieldLabel htmlFor="busqueda">Buscar artículo</FieldLabel>
        <InputGroup>
          <InputGroupInput
            id="busqueda"
            name="busqueda"
            placeholder="Buscar artículo..."
          />
          <InputGroupAddon>
            <Search className="size-3.5" />
          </InputGroupAddon>
        </InputGroup>
      </Field>
    </div>
  );
}

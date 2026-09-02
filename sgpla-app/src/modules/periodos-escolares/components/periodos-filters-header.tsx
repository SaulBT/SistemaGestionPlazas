"use client";

import { Search } from "lucide-react";

import { Field, FieldLabel } from "@/components/ui/field";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from "@/components/ui/input-group";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { periodosOptions } from "../data";

export function PeriodosFiltersHeader() {
  return (
    <div className="flex flex-col items-end gap-3 sm:flex-row sm:justify-between">
      <Field className="w-full sm:max-w-xs">
        <FieldLabel htmlFor="anioBusqueda">Buscar por año</FieldLabel>
        <InputGroup>
          <InputGroupInput
            id="anioBusqueda"
            name="anioFiltro"
            inputMode="numeric"
            maxLength={4}
            placeholder="Buscar por año..."
          />
          <InputGroupAddon>
            <Search className="size-3.5" />
          </InputGroupAddon>
        </InputGroup>
      </Field>

      <div className="flex w-full flex-col gap-3 sm:w-auto sm:flex-row">
        <div className="w-full shrink-0 sm:w-48">
          <Field>
            <FieldLabel htmlFor="periodoFiltro">Periodo</FieldLabel>
            <Select name="periodoFiltro">
              <SelectTrigger id="periodoFiltro" className="w-full">
                <SelectValue placeholder="Todos los periodos" />
              </SelectTrigger>
              <SelectContent>
                {periodosOptions.map((periodo) => (
                  <SelectItem key={periodo} value={periodo}>
                    {periodo}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </Field>
        </div>
      </div>
    </div>
  );
}

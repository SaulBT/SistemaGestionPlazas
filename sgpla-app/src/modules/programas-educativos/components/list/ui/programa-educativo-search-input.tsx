"use client";
import { Search } from "lucide-react";
import { Field, FieldLabel } from "@/components/ui/field";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from "@/components/ui/input-group";
type Props = { value: string; onChange: (busqueda: string) => void };
export function ProgramaEducativoSearchInput({ value, onChange }: Props) {
  return (
    <Field className="w-full sm:max-w-xs">
      <FieldLabel htmlFor="busqueda">Buscar programa educativo</FieldLabel>
      <InputGroup>
        <InputGroupInput
          id="busqueda"
          value={value}
          placeholder="Buscar programa educativo..."
          onChange={(event) => onChange(event.target.value)}
        />
        <InputGroupAddon>
          <Search className="size-4" />
        </InputGroupAddon>
      </InputGroup>
    </Field>
  );
}

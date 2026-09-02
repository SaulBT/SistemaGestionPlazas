"use client";

import { Search } from "lucide-react";
import { Field, FieldLabel } from "@/components/ui/field";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from "@/components/ui/input-group";
import { academicAreas, academicEntities, regions } from "../data";
import { UserSelectField } from "./user-select-field";

export function UsersFiltersHeader() {
  return (
    <div className="flex flex-col items-end gap-3 sm:flex-row sm:justify-between">
      <Field className="w-full sm:max-w-xs">
        <FieldLabel htmlFor="busqueda">Buscar usuario</FieldLabel>
        <InputGroup>
          <InputGroupInput
            id="busqueda"
            name="busqueda"
            placeholder="Buscar usuario..."
          />
          <InputGroupAddon>
            <Search className="size-3.5" />
          </InputGroupAddon>
        </InputGroup>
      </Field>

      <div className="flex w-full flex-col gap-3 sm:w-auto sm:flex-row">
        <div className="w-full shrink-0 sm:w-48">
          <UserSelectField
            id="region"
            label="Región"
            name="region"
            options={regions}
          />
        </div>
        <div className="w-full shrink-0 sm:w-48">
          <UserSelectField
            id="idAreaAcademica"
            label="Área Académica"
            name="idAreaAcademica"
            options={academicAreas}
          />
        </div>
        <div className="w-full shrink-0 sm:w-48">
          <UserSelectField
            id="idEntidadAcademica"
            label="Entidad Académica"
            name="idEntidadAcademica"
            options={academicEntities}
          />
        </div>
      </div>
    </div>
  );
}

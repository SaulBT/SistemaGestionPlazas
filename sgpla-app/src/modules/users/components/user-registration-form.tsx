import Link from "next/link";

import { Button } from "@/components/ui/button";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { academicAreas, academicEntities, regions, roles } from "../data";
import { UserSelectField } from "./user-select-field";

export function UserRegistrationForm() {
  return (
    <form action="/Usuarios/CrearUsuario" method="post" className="space-y-8">
      <FieldGroup className="gap-6">
        <Field>
          <FieldLabel htmlFor="nombre">Nombre:</FieldLabel>
          <Input id="nombre" name="Nombre" />
        </Field>

        <div className="grid gap-6 md:grid-cols-3">
          <Field>
            <FieldLabel htmlFor="cargo">Cargo:</FieldLabel>
            <Input id="cargo" name="Cargo" />
          </Field>
          <Field>
            <FieldLabel htmlFor="correo">Correo:</FieldLabel>
            <Input id="correo" name="Correo" type="email" />
          </Field>
          <UserSelectField id="rol" label="Rol:" name="Rol" options={roles} />
        </div>

        <div className="grid gap-6 md:grid-cols-3">
          <UserSelectField
            id="idAreaAcademica"
            label="Área Académica"
            name="IdAreaAcademica"
            options={academicAreas}
          />
          <UserSelectField id="region" label="Región" name="Region" options={regions} />
          <UserSelectField
            id="idEntidadAcademica"
            label="Entidad Académica"
            name="IdEntidadAcademica"
            options={academicEntities}
          />
        </div>
      </FieldGroup>

      <div className="flex justify-end gap-3">
        <Button nativeButton={false} render={<Link href="/Usuarios" />} variant="secondary">
          Cancelar
        </Button>
        <Button type="submit">Guardar</Button>
      </div>
    </form>
  );
}

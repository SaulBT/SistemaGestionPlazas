import Link from "next/link";

import { Button } from "@/components/ui/button";
import { Field, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";

import type { DireccionAreaAcademica } from "../data";

type DireccionFormViewProps = {
  direccion?: DireccionAreaAcademica;
  readOnly?: boolean;
};

export function DireccionFormView({ direccion, readOnly = false }: DireccionFormViewProps) {
  return (
    <div className="mx-auto max-w-3xl space-y-8 pt-24">
      <div className="grid gap-6">
        <Field>
          <FieldLabel htmlFor="nombre">Nombre</FieldLabel>
          <Input id="nombre" name="nombre" defaultValue={direccion?.nombre} maxLength={100} disabled={readOnly} />
        </Field>
        <div className="grid gap-6 sm:grid-cols-2">
          <Field>
            <FieldLabel htmlFor="telefono">Teléfono</FieldLabel>
            <Input id="telefono" name="telefono" defaultValue={direccion?.telefono} maxLength={10} disabled={readOnly} />
          </Field>
          <Field>
            <FieldLabel htmlFor="extension">Extensión</FieldLabel>
            <Input id="extension" name="extension" defaultValue={direccion?.extension} maxLength={5} disabled={readOnly} />
          </Field>
        </div>
      </div>

      <div className="flex justify-end gap-2">
        <Button nativeButton={false} render={<Link href="/DireccionesAreaAcademica" />} variant="outline">
          {readOnly ? "Regresar" : "Cancelar"}
        </Button>
        {!readOnly && <Button type="button">Guardar</Button>}
      </div>
    </div>
  );
}

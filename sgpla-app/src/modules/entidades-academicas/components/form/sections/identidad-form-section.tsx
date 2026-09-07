import { FormSection } from "@/components/form-section";
import { TextField } from "@/components/text-field";
import { FieldError } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { EntidadFormSectionProps } from "./entidad-form-section.types";

export function IdentidadFormSection({
  values,
  readOnly,
  onChange,
  errorFor,
}: EntidadFormSectionProps) {
  return (
    <FormSection
      title="Identidad"
      description="Información principal de la entidad académica."
    >
      <div className="flex flex-col gap-5">
        <TextField id="clave" required>
          <Label>Clave</Label>
          <Input
            name="clave"
            value={values.clave}
            onChange={(event) => onChange("clave", event.target.value)}
            maxLength={5}
            pattern="[0-9]{5}"
            inputMode="numeric"
            disabled={readOnly}
            aria-invalid={Boolean(errorFor("clave"))}
          />
          <FieldError>{errorFor("clave")}</FieldError>
        </TextField>
        <TextField id="nombre" required>
          <Label>Nombre</Label>
          <Input
            name="nombre"
            value={values.nombre}
            onChange={(event) => onChange("nombre", event.target.value)}
            maxLength={100}
            disabled={readOnly}
            aria-invalid={Boolean(errorFor("nombre"))}
          />
          <FieldError>{errorFor("nombre")}</FieldError>
        </TextField>
      </div>
    </FormSection>
  );
}
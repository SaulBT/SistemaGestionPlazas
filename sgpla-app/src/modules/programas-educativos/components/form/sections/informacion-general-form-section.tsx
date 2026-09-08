import { FormSection } from "@/components/form-section";
import { TextField } from "@/components/text-field";
import { FieldError } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { ProgramaFormSectionProps } from "./programa-form-section.types";

export function InformacionGeneralFormSection({
  values,
  readOnly,
  onChange,
  errorFor,
}: ProgramaFormSectionProps) {
  return (
    <FormSection
      title="Información general"
      description="Datos principales del programa educativo."
    >
      <div className="flex flex-col gap-5">
        <div className="grid gap-5 sm:grid-cols-[minmax(8rem,1fr)_3fr]">
          <TextField id="clave" required>
            <Label>Clave</Label>
            <Input
              name="clave"
              value={values.clave}
              onChange={(event) => onChange("clave", event.target.value)}
              maxLength={5}
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
              maxLength={94}
              disabled={readOnly}
              aria-invalid={Boolean(errorFor("nombre"))}
            />
            <FieldError>{errorFor("nombre")}</FieldError>
          </TextField>
        </div>
        <TextField id="campus" required>
          <Label>Campus</Label>
          <Input
            name="campus"
            value={values.campus}
            onChange={(event) => onChange("campus", event.target.value)}
            maxLength={100}
            disabled={readOnly}
            aria-invalid={Boolean(errorFor("campus"))}
          />
          <FieldError>{errorFor("campus")}</FieldError>
        </TextField>
      </div>
    </FormSection>
  );
}

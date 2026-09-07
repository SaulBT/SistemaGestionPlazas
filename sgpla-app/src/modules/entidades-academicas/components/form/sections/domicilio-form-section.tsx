import { FormSection } from "@/components/form-section";
import { TextField } from "@/components/text-field";
import { FieldError } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { EntidadFormSectionProps } from "./entidad-form-section.types";

export function DomicilioFormSection({
  values,
  readOnly,
  onChange,
  errorFor,
}: EntidadFormSectionProps) {
  return (
    <FormSection
      title="Domicilio"
      description="Ubicación física de la entidad académica."
    >
      <div className="flex flex-col gap-5">
        <TextField id="calleNumero" required>
          <Label>Domicilio (Calle y Número)</Label>
          <Input
            name="calleNumero"
            value={values.calleNumero}
            onChange={(event) => onChange("calleNumero", event.target.value)}
            maxLength={150}
            disabled={readOnly}
            aria-invalid={Boolean(errorFor("calleNumero"))}
          />
          <FieldError>{errorFor("calleNumero")}</FieldError>
        </TextField>
        <div className="grid grid-cols-[3fr_1fr] gap-3">
          <TextField id="colonia" required>
            <Label>Colonia</Label>
            <Input
              name="colonia"
              value={values.colonia}
              onChange={(event) => onChange("colonia", event.target.value)}
              maxLength={100}
              disabled={readOnly}
              aria-invalid={Boolean(errorFor("colonia"))}
            />
            <FieldError>{errorFor("colonia")}</FieldError>
          </TextField>
          <TextField id="cp" required>
            <Label>C.P.</Label>
            <Input
              name="cp"
              value={values.cp}
              onChange={(event) => onChange("cp", event.target.value)}
              maxLength={5}
              disabled={readOnly}
              aria-invalid={Boolean(errorFor("cp"))}
            />
            <FieldError>{errorFor("cp")}</FieldError>
          </TextField>
        </div>
        <TextField id="municipio" required>
          <Label>Municipio</Label>
          <Input
            name="municipio"
            value={values.municipio}
            onChange={(event) => onChange("municipio", event.target.value)}
            maxLength={100}
            disabled={readOnly}
            aria-invalid={Boolean(errorFor("municipio"))}
          />
          <FieldError>{errorFor("municipio")}</FieldError>
        </TextField>
      </div>
    </FormSection>
  );
}
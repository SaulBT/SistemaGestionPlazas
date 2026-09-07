import { FormSection } from "@/components/form-section";
import { TextField } from "@/components/text-field";
import { FieldError } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { EntidadFormSectionProps } from "./entidad-form-section.types";

export function ContactoFormSection({
  values,
  readOnly,
  onChange,
  errorFor,
}: EntidadFormSectionProps) {
  return (
    <FormSection
      title="Contacto"
      description="Datos para contactar a la entidad académica."
    >
      <div className="flex flex-col gap-5">
        <TextField id="telefono" required>
          <Label>Teléfono</Label>
          <Input
            name="telefono"
            value={values.telefono}
            onChange={(event) => onChange("telefono", event.target.value)}
            maxLength={30}
            disabled={readOnly}
            aria-invalid={Boolean(errorFor("telefono"))}
          />
          <FieldError>{errorFor("telefono")}</FieldError>
        </TextField>
        <TextField id="extension" required>
          <Label>Extensión</Label>
          <Input
            name="extension"
            value={values.extension}
            onChange={(event) => onChange("extension", event.target.value)}
            maxLength={5}
            disabled={readOnly}
            aria-invalid={Boolean(errorFor("extension"))}
          />
          <FieldError>{errorFor("extension")}</FieldError>
        </TextField>
      </div>
    </FormSection>
  );
}
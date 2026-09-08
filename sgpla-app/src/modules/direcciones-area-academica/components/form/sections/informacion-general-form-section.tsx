import { FormSection } from "@/components/form-section";
import { TextField } from "@/components/text-field";
import { FieldError } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { DireccionFormSectionProps } from "./direccion-form-section.types";

export function InformacionGeneralFormSection({
  values,
  readOnly,
  onChange,
  errorFor,
}: DireccionFormSectionProps) {
  return (
    <FormSection
      title="Información general"
      description="Datos principales de la dirección de área académica."
    >
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
    </FormSection>
  );
}
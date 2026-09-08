import { FormSection } from "@/components/form-section";
import { FormSelectField } from "@/components/form-select-field";
import { REGIONES } from "../../../domain/entidad-academica";
import type { ClasificacionFormSectionProps } from "./entidad-form-section.types";

export function ClasificacionFormSection({
  values,
  areas,
  areasLoading,
  readOnly,
  onChange,
  errorFor,
}: ClasificacionFormSectionProps) {
  return (
    <FormSection
      title="Clasificación"
      description="Área académica y región a la que pertenece."
    >
      <div className="flex flex-col gap-5">
        <FormSelectField
          id="idAreaAcademica"
          label="Área Académica"
          value={values.idAreaAcademica}
          options={areas}
          onChange={(value) => onChange("idAreaAcademica", value)}
          loading={areasLoading}
          disabled={readOnly}
          required
          error={errorFor("idAreaAcademica")}
        />
        <FormSelectField
          id="region"
          label="Región"
          value={values.region}
          options={REGIONES.map((region) => ({
            value: region,
            label: region,
          }))}
          onChange={(value) => onChange("region", value)}
          disabled={readOnly}
          required
          error={errorFor("region")}
        />
      </div>
    </FormSection>
  );
}
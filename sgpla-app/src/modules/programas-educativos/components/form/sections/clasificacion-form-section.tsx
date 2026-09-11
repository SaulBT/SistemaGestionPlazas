import { FormSection } from "@/components/form-section";
import { FormSelectField } from "@/components/form-select-field";
import { REGIONES } from "../../../domain/programa-educativo";
import type { ProgramaFormSectionProps } from "./programa-form-section.types";

type Props = ProgramaFormSectionProps & {
  areas: Array<{ value: string; label: string }>;
  areasLoading: boolean;
  entidades: Array<{ value: string; label: string }>;
  entidadesLoading: boolean;
};

export function ClasificacionFormSection({
  values,
  areas,
  areasLoading,
  entidades,
  entidadesLoading,
  readOnly,
  onChange,
  errorFor,
}: Props) {
  return (
    <FormSection
      title="Clasificación"
      description="Ubicación académica a la que pertenece el programa."
    >
      <div className="flex flex-col gap-5">
        <div className="grid gap-5 sm:grid-cols-2">
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
        </div>
        <FormSelectField
          id="idEntidadAcademica"
          label="Entidad Académica"
          value={values.idEntidadAcademica}
          options={entidades}
          onChange={(value) => onChange("idEntidadAcademica", value)}
          loading={entidadesLoading}
          disabled={readOnly}
          required
          error={errorFor("idEntidadAcademica")}
        />
      </div>
    </FormSection>
  );
}

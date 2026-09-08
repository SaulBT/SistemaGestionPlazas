import type { ProgramaFormValues } from "../../application/programa-form.model";
import { ClasificacionFormSection } from "./sections/clasificacion-form-section";
import { InformacionGeneralFormSection } from "./sections/informacion-general-form-section";

type Props = {
  values: ProgramaFormValues;
  areas: Array<{ value: string; label: string }>;
  areasLoading: boolean;
  entidades: Array<{ value: string; label: string }>;
  entidadesLoading: boolean;
  readOnly: boolean;
  onChange: <K extends keyof ProgramaFormValues>(
    field: K,
    value: ProgramaFormValues[K],
  ) => void;
  errorFor: (field: keyof ProgramaFormValues) => string | undefined;
};

export function ProgramaFormFields({
  values,
  areas,
  areasLoading,
  entidades,
  entidadesLoading,
  readOnly,
  onChange,
  errorFor,
}: Props) {
  const sectionProps = { values, readOnly, onChange, errorFor };
  return (
    <>
      <InformacionGeneralFormSection {...sectionProps} />
      <ClasificacionFormSection
        {...sectionProps}
        areas={areas}
        areasLoading={areasLoading}
        entidades={entidades}
        entidadesLoading={entidadesLoading}
      />
    </>
  );
}

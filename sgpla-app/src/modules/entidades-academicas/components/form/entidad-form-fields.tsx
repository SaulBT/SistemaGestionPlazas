import type { EntidadFormValues } from "../../application/entidad-form.model";
import { ClasificacionFormSection } from "./sections/clasificacion-form-section";
import { ContactoFormSection } from "./sections/contacto-form-section";
import { DomicilioFormSection } from "./sections/domicilio-form-section";
import { IdentidadFormSection } from "./sections/identidad-form-section";

type EntidadFormFieldsProps = {
  values: EntidadFormValues;
  areas: Array<{ value: string; label: string }>;
  areasLoading: boolean;
  readOnly: boolean;
  onChange: <K extends keyof EntidadFormValues>(
    field: K,
    value: EntidadFormValues[K],
  ) => void;
  errorFor: (field: keyof EntidadFormValues) => string | undefined;
};

export function EntidadFormFields({
  values,
  areas,
  areasLoading,
  readOnly,
  onChange,
  errorFor,
}: EntidadFormFieldsProps) {
  const sectionProps = { values, readOnly, onChange, errorFor };

  return (
    <>
      <IdentidadFormSection {...sectionProps} />
      <ClasificacionFormSection
        {...sectionProps}
        areas={areas}
        areasLoading={areasLoading}
      />
      <DomicilioFormSection {...sectionProps} />
      <ContactoFormSection {...sectionProps} />
    </>
  );
}
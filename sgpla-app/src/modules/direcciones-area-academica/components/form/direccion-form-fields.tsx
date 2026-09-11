import type { DireccionFormValues } from "../../application/direccion-form.model";
import { ContactoFormSection } from "./sections/contacto-form-section";
import { InformacionGeneralFormSection } from "./sections/informacion-general-form-section";

type Props = {
  values: DireccionFormValues;
  readOnly: boolean;
  onChange: <K extends keyof DireccionFormValues>(
    field: K,
    value: DireccionFormValues[K],
  ) => void;
  errorFor: (field: keyof DireccionFormValues) => string | undefined;
};

export function DireccionFormFields({
  values,
  readOnly,
  onChange,
  errorFor,
}: Props) {
  const sectionProps = { values, readOnly, onChange, errorFor };

  return (
    <>
      <InformacionGeneralFormSection {...sectionProps} />
      <ContactoFormSection {...sectionProps} />
    </>
  );
}
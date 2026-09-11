import type { DireccionFormValues } from "../../../application/direccion-form.model";

export type DireccionFormSectionProps = {
  values: DireccionFormValues;
  readOnly: boolean;
  onChange: <K extends keyof DireccionFormValues>(
    field: K,
    value: DireccionFormValues[K],
  ) => void;
  errorFor: (field: keyof DireccionFormValues) => string | undefined;
};
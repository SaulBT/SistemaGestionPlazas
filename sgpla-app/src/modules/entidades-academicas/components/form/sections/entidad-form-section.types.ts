import type { EntidadFormValues } from "../../../application/entidad-form.model";

export type EntidadFormSectionProps = {
  values: EntidadFormValues;
  readOnly: boolean;
  onChange: <K extends keyof EntidadFormValues>(
    field: K,
    value: EntidadFormValues[K],
  ) => void;
  errorFor: (field: keyof EntidadFormValues) => string | undefined;
};

export type ClasificacionFormSectionProps = EntidadFormSectionProps & {
  areas: Array<{ value: string; label: string }>;
  areasLoading: boolean;
};
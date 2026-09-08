import type { ProgramaFormValues } from "../../../application/programa-form.model";

export type ProgramaFormSectionProps = {
  values: ProgramaFormValues;
  readOnly: boolean;
  onChange: <K extends keyof ProgramaFormValues>(
    field: K,
    value: ProgramaFormValues[K],
  ) => void;
  errorFor: (field: string) => string | undefined;
};

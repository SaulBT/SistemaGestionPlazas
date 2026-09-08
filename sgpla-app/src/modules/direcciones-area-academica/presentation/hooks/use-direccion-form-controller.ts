"use client";

import { useState } from "react";
import {
  HttpClientError,
  type ValidationErrors,
} from "@/shared/api/http-client";
import { toZodValidationErrors } from "@/shared/forms/zod-validation-errors";
import {
  hasDireccionFormChanges,
  toDireccionFormValues,
  toGuardarDireccionAreaAcademicaInput,
  type DireccionFormValues,
} from "../../application/direccion-form.model";
import {
  direccionFormRequiredFields,
  direccionFormSchema,
} from "../../application/direccion-form.validation";
import type { DireccionAreaAcademica } from "../../domain/direccion-area-academica";
import { useGuardarDireccionAreaAcademica } from "../direcciones-area-academica.queries";

type Props = {
  direccion?: DireccionAreaAcademica;
};

type SaveResult =
  | { status: "invalid" }
  | { status: "saved"; operation: "created" | "updated" }
  | { status: "error"; error: unknown };

export function useDireccionFormController({ direccion }: Props) {
  const guardarMutation = useGuardarDireccionAreaAcademica();
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>(
    {},
  );
  const [formValues, setFormValues] = useState<DireccionFormValues>(() =>
    toDireccionFormValues(direccion),
  );
  const originalFormValues = toDireccionFormValues(direccion);
  const hasEmptyRequiredFields = direccionFormRequiredFields.some(
    (field) => formValues[field].trim().length === 0,
  );
  const isDirty = hasDireccionFormChanges(formValues, originalFormValues);
  const isSubmitDisabled =
    hasEmptyRequiredFields || (Boolean(direccion) && !isDirty);

  function updateField<K extends keyof DireccionFormValues>(
    field: K,
    value: DireccionFormValues[K],
  ) {
    setFormValues((current) => ({ ...current, [field]: value }));
  }

  function validateForm() {
    const validation = direccionFormSchema.safeParse(formValues);

    if (validation.success) {
      setValidationErrors({});
      return true;
    }

    setValidationErrors(toZodValidationErrors(validation.error));
    return false;
  }

  async function saveForm(): Promise<SaveResult> {
    if (!validateForm()) return { status: "invalid" };

    try {
      await guardarMutation.mutateAsync({
        input: toGuardarDireccionAreaAcademicaInput(formValues),
        direccionExistente: direccion,
      });

      return {
        status: "saved",
        operation: direccion ? "updated" : "created",
      };
    } catch (error) {
      if (error instanceof HttpClientError) {
        setValidationErrors(error.validationErrors ?? {});
      }

      return { status: "error", error };
    }
  }

  return {
    errorFor: (field: keyof DireccionFormValues) =>
      validationErrors[field]?.[0],
    formValues,
    isDirty,
    isSaving: guardarMutation.isPending,
    isSubmitDisabled,
    saveForm,
    updateField,
    validateForm,
  };
}
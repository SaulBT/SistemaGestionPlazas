"use client";

import { useState } from "react";
import {
  HttpClientError,
  type ValidationErrors,
} from "@/shared/api/http-client";
import { toZodValidationErrors } from "@/shared/forms/zod-validation-errors";
import type { EntidadAcademica } from "../../domain/entidad-academica";
import {
  useAreasAcademicas,
  useGuardarEntidadAcademica,
} from "../entidades-academicas.queries";
import {
  hasEntidadFormChanges,
  toEntidadFormValues,
  toGuardarEntidadAcademicaInput,
  type EntidadFormValues,
} from "../../application/entidad-form.model";
import {
  entidadAcademicaFormSchema,
  entidadAcademicaRequiredFields,
} from "../../application/entidad-form.validation";

type Props = {
  entidad?: EntidadAcademica;
};

type SaveResult =
  | { status: "invalid" }
  | { status: "saved"; operation: "created" | "updated" }
  | { status: "error"; error: unknown };

export function useEntidadFormController({ entidad }: Props) {
  const areasQuery = useAreasAcademicas();
  const guardarMutation = useGuardarEntidadAcademica();
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>(
    {},
  );
  const [formValues, setFormValues] = useState<EntidadFormValues>(() =>
    toEntidadFormValues(entidad),
  );
  const originalFormValues = toEntidadFormValues(entidad);
  const hasEmptyRequiredFields = entidadAcademicaRequiredFields.some(
    (field) => formValues[field].trim().length === 0,
  );
  const isDirty = hasEntidadFormChanges(formValues, originalFormValues);
  const isSubmitDisabled =
    hasEmptyRequiredFields || (Boolean(entidad) && !isDirty);
  const areas = (areasQuery.data?.items ?? []).map((area) => ({
    value: area.idAreaAcademica.toString(),
    label: area.nombre,
  }));

  function updateField<K extends keyof EntidadFormValues>(
    field: K,
    value: EntidadFormValues[K],
  ) {
    setFormValues((current) => ({ ...current, [field]: value }));
  }

  function validateForm() {
    const validation = entidadAcademicaFormSchema.safeParse(formValues);

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
        input: toGuardarEntidadAcademicaInput(formValues),
        entidadExistente: entidad,
      });

      return {
        status: "saved",
        operation: entidad ? "updated" : "created",
      };
    } catch (error) {
      if (error instanceof HttpClientError) {
        setValidationErrors(
          error.validationErrors ??
            (error.status === 409 ? { clave: [error.message] } : {}),
        );
      }

      return { status: "error", error };
    }
  }

  return {
    areas,
    areasQuery,
    errorFor: (field: keyof EntidadFormValues) => validationErrors[field]?.[0],
    formValues,
    isDirty,
    isSaving: guardarMutation.isPending,
    isSubmitDisabled,
    saveForm,
    updateField,
    validateForm,
  };
}

"use client";

import { useMemo, useState } from "react";
import {
  HttpClientError,
  type ValidationErrors,
} from "@/shared/api/http-client";
import { toZodValidationErrors } from "@/shared/forms/zod-validation-errors";
import {
  hasProgramaFormChanges,
  toGuardarProgramaEducativoInput,
  toProgramaFormValues,
  type ProgramaFormValues,
} from "../../application/programa-form.model";
import {
  programaFormRequiredFields,
  programaFormSchema,
} from "../../application/programa-form.validation";
import type { ProgramaEducativo } from "../../domain/programa-educativo";
import {
  useAreasAcademicas,
  useEntidadesAcademicas,
  useGuardarProgramaEducativo,
} from "../programas-educativos.queries";

type Props = {
  programa?: ProgramaEducativo;
};

type SaveResult =
  | { status: "invalid" }
  | { status: "saved"; operation: "created" | "updated" }
  | { status: "error"; error: unknown };

export function useProgramaFormController({ programa }: Props) {
  const areasQuery = useAreasAcademicas();
  const entidadesQuery = useEntidadesAcademicas();
  const guardarMutation = useGuardarProgramaEducativo();
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>(
    {},
  );
  const [formValues, setFormValues] = useState<ProgramaFormValues>(() =>
    toProgramaFormValues(programa),
  );
  const originalFormValues = toProgramaFormValues(programa);
  const hasEmptyRequiredFields = programaFormRequiredFields.some(
    (field) => formValues[field].trim().length === 0,
  );
  const isDirty = hasProgramaFormChanges(formValues, originalFormValues);
  const isSubmitDisabled =
    hasEmptyRequiredFields || (Boolean(programa) && !isDirty);
  const areas = useMemo(
    () =>
      (areasQuery.data?.items ?? []).map((area) => ({
        value: area.idAreaAcademica.toString(),
        label: area.nombre,
      })),
    [areasQuery.data],
  );
  const entidades = useMemo(
    () =>
      (entidadesQuery.data?.items ?? []).filter(
        (entidad) =>
          (!formValues.region || entidad.region === formValues.region) &&
          (!formValues.idAreaAcademica ||
            entidad.idAreaAcademica.toString() === formValues.idAreaAcademica),
      ),
    [entidadesQuery.data, formValues.idAreaAcademica, formValues.region],
  );

  function updateField<K extends keyof ProgramaFormValues>(
    field: K,
    value: ProgramaFormValues[K],
  ) {
    if (field === "idEntidadAcademica") {
      const entidad = (entidadesQuery.data?.items ?? []).find(
        (item) => item.idEntidadAcademica.toString() === value,
      );
      setFormValues((current) => ({
        ...current,
        idEntidadAcademica: value,
        idAreaAcademica: entidad?.idAreaAcademica.toString() ?? "",
        region: entidad?.region ?? "",
      }));
      return;
    }

    if (field === "region") {
      setFormValues((current) => ({
        ...current,
        region: value,
        idAreaAcademica: "",
        idEntidadAcademica: "",
      }));
      return;
    }

    if (field === "idAreaAcademica") {
      setFormValues((current) => ({
        ...current,
        idAreaAcademica: value,
        idEntidadAcademica: "",
      }));
      return;
    }

    setFormValues((current) => ({ ...current, [field]: value }));
  }

  function validateForm() {
    const validation = programaFormSchema.safeParse(formValues);

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
        input: toGuardarProgramaEducativoInput(formValues),
        programaExistente: programa,
      });

      return {
        status: "saved",
        operation: programa ? "updated" : "created",
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
    entidades,
    entidadesQuery,
    errorFor: (field: keyof ProgramaFormValues) => validationErrors[field]?.[0],
    formValues,
    isDirty,
    isSaving: guardarMutation.isPending,
    isSubmitDisabled,
    saveForm,
    updateField,
    validateForm,
  };
}

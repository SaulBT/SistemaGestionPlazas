"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "@/components/ui/toast";
import { useMinimumLoading } from "@/shared/hooks/use-minimum-loading";
import { MINIMUM_LOADING_DURATION_MS } from "@/shared/constants/loading";
import {
  HttpClientError,
  getErrorMessage,
  type ValidationErrors,
} from "@/shared/api/http-client";
import {
  useActualizarEntidadAcademica,
  useAreasAcademicas,
  useCrearEntidadAcademica,
} from "../presentation/entidades-academicas.queries";
import {
  toEntidadFormValues,
  toGuardarEntidadAcademicaInput,
  type EntidadFormValues,
} from "./entidad-form.model";
import {
  entidadAcademicaFormSchema,
  entidadAcademicaRequiredFields,
  toValidationErrors,
} from "./entidad-form.validation";
import type { EntidadAcademica } from "../domain/entidad-academica";

type UseEntidadFormProps = {
  entidad?: EntidadAcademica;
};

export function useEntidadForm({ entidad }: UseEntidadFormProps) {
  const router = useRouter();
  const areasQuery = useAreasAcademicas();
  const crearMutation = useCrearEntidadAcademica();
  const actualizarMutation = useActualizarEntidadAcademica();
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>(
    {},
  );
  const [formError, setFormError] = useState<string>();
  const [formValues, setFormValues] = useState<EntidadFormValues>(() =>
    toEntidadFormValues(entidad),
  );

  const areas = (areasQuery.data?.items ?? []).map((area) => ({
    value: area.idAreaAcademica.toString(),
    label: area.nombre,
  }));
  const mutationIsSaving =
    crearMutation.isPending || actualizarMutation.isPending;
  const isSaving = useMinimumLoading(
    mutationIsSaving,
    MINIMUM_LOADING_DURATION_MS,
    0,
  );
  const originalFormValues = toEntidadFormValues(entidad);
  const formFields = Object.keys(formValues) as Array<keyof EntidadFormValues>;
  const hasEmptyRequiredFields = entidadAcademicaRequiredFields.some(
    (field) => formValues[field].trim().length === 0,
  );
  const hasChanges = formFields.some(
    (field) => formValues[field] !== originalFormValues[field],
  );
  const isSubmitDisabled =
    hasEmptyRequiredFields || (Boolean(entidad) && !hasChanges);

  function updateField<K extends keyof EntidadFormValues>(
    field: K,
    value: EntidadFormValues[K],
  ) {
    setFormValues((current) => ({ ...current, [field]: value }));
  }

  async function submitForm() {
    setValidationErrors({});
    setFormError(undefined);

    const validation = entidadAcademicaFormSchema.safeParse(formValues);

    if (!validation.success) {
      setValidationErrors(toValidationErrors(validation.error));
      return;
    }

    try {
      const savingStartedAt = Date.now();
      const input = toGuardarEntidadAcademicaInput(formValues);

      if (entidad) {
        await actualizarMutation.mutateAsync({
          idEntidadAcademica: entidad.idEntidadAcademica,
          input,
        });
      } else {
        await crearMutation.mutateAsync(input);
      }

      const remainingMs = Math.max(
        0,
        MINIMUM_LOADING_DURATION_MS - (Date.now() - savingStartedAt),
      );
      if (remainingMs > 0) {
        await new Promise((resolve) => window.setTimeout(resolve, remainingMs));
      }

      toast.add({
        type: "success",
        title: entidad
          ? "Entidad académica actualizada"
          : "Entidad académica registrada",
        description: entidad
          ? "Los cambios se guardaron correctamente."
          : "La entidad académica se registró correctamente.",
      });

      router.push("/EntidadesAcademicas");
      router.refresh();
    } catch (error) {
      if (error instanceof HttpClientError) {
        setValidationErrors(error.validationErrors ?? {});
      }

      setFormError(getErrorMessage(error));
    }
  }

  return {
    areas,
    areasQuery,
    formError,
    formValues,
    isSubmitDisabled,
    isSaving,
    submitForm,
    updateField,
    validationErrors,
    errorFor: (field: string) => validationErrors[field]?.[0],
  };
}

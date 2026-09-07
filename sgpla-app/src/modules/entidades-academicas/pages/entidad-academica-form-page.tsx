"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Plus } from "lucide-react";
import { Form, FormContent } from "@/components/form";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";
import { getErrorMessage } from "@/shared/api/http-client";
import type { EntidadAcademica } from "../domain/entidad-academica";
import { useEntidadFormController } from "../presentation/hooks/use-entidad-form-controller";
import { useUnsavedChangesWarning } from "../presentation/hooks/use-unsaved-changes-warning";
import { EntidadFormFields } from "../components/form/entidad-form-fields";
import { EntidadFormExitConfirmationDialog } from "../components/form/ui/entidad-form-exit-confirmation-dialog";
import { showEntidadAcademicaSavedToast } from "../components/form/ui/entidad-academica-form-notifications";
import { EntidadFormSaveConfirmationDialog } from "../components/form/ui/entidad-form-save-confirmation-dialog";

type EntidadAcademicaFormPageProps = {
  entidad?: EntidadAcademica;
  readOnly?: boolean;
  title: string;
  description: string;
};

const ENTIDAD_FORM_ID = "entidad-academica-form";

export function EntidadAcademicaFormPage({
  entidad,
  readOnly = false,
  title,
  description,
}: EntidadAcademicaFormPageProps) {
  const router = useRouter();
  const {
    areas,
    areasQuery,
    errorFor,
    formError,
    formValues,
    isSubmitDisabled,
    isSaving,
    saveForm: submitForm,
    updateField,
    isDirty,
    validateForm,
  } = useEntidadFormController({ entidad });
  const [confirmationOpen, setConfirmationOpen] = useState(false);
  const [exitConfirmationOpen, setExitConfirmationOpen] = useState(false);
  useUnsavedChangesWarning(!readOnly && isDirty);

  function handleExit() {
    if (readOnly) {
      router.push("/EntidadesAcademicas");
      return;
    }

    if (isDirty) {
      setExitConfirmationOpen(true);
      return;
    }

    router.push("/EntidadesAcademicas");
  }

  async function saveForm() {
    setConfirmationOpen(false);
    const result = await submitForm();

    if (result.status !== "saved") return;

    showEntidadAcademicaSavedToast(result.operation);
    router.push("/EntidadesAcademicas");
    router.refresh();
  }

  return (
    <>
      <PageHeader
        title={title}
        description={description}
        onBack={handleExit}
        actions={
          <>
            {readOnly ? (
              <Button
                nativeButton={false}
                render={<Link href="/EntidadesAcademicas" />}
                variant="outline"
              >
                Regresar
              </Button>
            ) : (
              <Button type="button" variant="outline" onClick={handleExit}>
                Cancelar
              </Button>
            )}
            {!readOnly ? (
              <Button
                type="submit"
                form={ENTIDAD_FORM_ID}
                disabled={isSaving || areasQuery.isPending || isSubmitDisabled}
              >
                <Plus />
                {isSaving
                  ? "Guardando..."
                  : entidad
                    ? "Guardar cambios"
                    : "Registrar entidad"}
              </Button>
            ) : null}
          </>
        }
      />

      <Form
        id={ENTIDAD_FORM_ID}
        className="mx-0 max-w-3xl pt-24"
        onSubmit={
          readOnly
            ? undefined
            : (event) => {
                event.preventDefault();
                if (validateForm()) setConfirmationOpen(true);
              }
        }
      >
        <FormContent>
          <EntidadFormFields
            values={formValues}
            areas={areas}
            areasLoading={areasQuery.isPending}
            readOnly={readOnly}
            onChange={updateField}
            errorFor={errorFor}
          />
        </FormContent>

        {areasQuery.isError ? (
          <p className="text-sm font-medium text-destructive" role="alert">
            {getErrorMessage(areasQuery.error)}
          </p>
        ) : null}

        {formError ? (
          <p className="text-sm font-medium text-destructive" role="alert">
            {formError}
          </p>
        ) : null}
      </Form>

      {!readOnly ? (
        <>
          <EntidadFormSaveConfirmationDialog
            isEditing={Boolean(entidad)}
            open={confirmationOpen}
            onOpenChange={setConfirmationOpen}
            onConfirm={() => {
              void saveForm();
            }}
          />

          <EntidadFormExitConfirmationDialog
            isEditing={Boolean(entidad)}
            open={exitConfirmationOpen}
            onOpenChange={setExitConfirmationOpen}
            onConfirm={() => {
              setExitConfirmationOpen(false);
              router.push("/EntidadesAcademicas");
            }}
          />
        </>
      ) : null}
    </>
  );
}

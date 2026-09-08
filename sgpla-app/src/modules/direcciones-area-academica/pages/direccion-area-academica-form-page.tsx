"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Plus } from "lucide-react";
import { Form, FormContent } from "@/components/form";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";
import { MINIMUM_LOADING_DURATION_MS } from "@/shared/constants/loading";
import type { DireccionAreaAcademica } from "../domain/direccion-area-academica";
import { DireccionFormFields } from "../components/form/direccion-form-fields";
import { DireccionFormExitConfirmationDialog } from "../components/form/ui/direccion-form-exit-confirmation-dialog";
import { showDireccionAreaAcademicaSavedToast } from "../components/form/ui/direccion-area-academica-form-notifications";
import { DireccionFormSaveConfirmationDialog } from "../components/form/ui/direccion-form-save-confirmation-dialog";
import { useDireccionFormController } from "../presentation/hooks/use-direccion-form-controller";
import { useUnsavedChangesWarning } from "../presentation/hooks/use-unsaved-changes-warning";

type Props = {
  direccion?: DireccionAreaAcademica;
  readOnly?: boolean;
  title: string;
  description: string;
};

const DIRECCION_FORM_ID = "direccion-area-academica-form";

export function DireccionAreaAcademicaFormPage({
  direccion,
  readOnly = false,
  title,
  description,
}: Props) {
  const router = useRouter();
  const {
    errorFor,
    formValues,
    isDirty,
    isSaving,
    isSubmitDisabled,
    saveForm: submitForm,
    updateField,
    validateForm,
  } = useDireccionFormController({ direccion });
  const [confirmationOpen, setConfirmationOpen] = useState(false);
  const [exitConfirmationOpen, setExitConfirmationOpen] = useState(false);
  useUnsavedChangesWarning(!readOnly && isDirty);

  function handleExit() {
    if (readOnly) {
      router.push("/DireccionesAreaAcademica");
      return;
    }

    if (isDirty) {
      setExitConfirmationOpen(true);
      return;
    }

    router.push("/DireccionesAreaAcademica");
  }

  async function saveForm() {
    setConfirmationOpen(false);
    const result = await submitForm();

    if (result.status !== "saved") return;

    router.push("/DireccionesAreaAcademica");
    router.refresh();
    window.setTimeout(
      () => showDireccionAreaAcademicaSavedToast(result.operation),
      MINIMUM_LOADING_DURATION_MS,
    );
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
                render={<Link href="/DireccionesAreaAcademica" />}
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
                form={DIRECCION_FORM_ID}
                disabled={isSaving || isSubmitDisabled}
              >
                <Plus />
                {isSaving
                  ? "Guardando..."
                  : direccion
                    ? "Guardar cambios"
                    : "Registrar dirección"}
              </Button>
            ) : null}
          </>
        }
      />

      <Form
        id={DIRECCION_FORM_ID}
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
          <DireccionFormFields
            values={formValues}
            readOnly={readOnly}
            onChange={updateField}
            errorFor={errorFor}
          />
        </FormContent>
      </Form>

      {!readOnly ? (
        <>
          <DireccionFormSaveConfirmationDialog
            isEditing={Boolean(direccion)}
            open={confirmationOpen}
            onOpenChange={setConfirmationOpen}
            onConfirm={() => {
              void saveForm();
            }}
          />
          <DireccionFormExitConfirmationDialog
            isEditing={Boolean(direccion)}
            open={exitConfirmationOpen}
            onOpenChange={setExitConfirmationOpen}
            onConfirm={() => {
              setExitConfirmationOpen(false);
              router.push("/DireccionesAreaAcademica");
            }}
          />
        </>
      ) : null}
    </>
  );
}
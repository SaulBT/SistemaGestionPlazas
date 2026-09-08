"use client";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Plus } from "lucide-react";
import { Form, FormContent } from "@/components/form";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";
import { getErrorMessage } from "@/shared/api/http-client";
import { MINIMUM_LOADING_DURATION_MS } from "@/shared/constants/loading";
import { ProgramaFormFields } from "../components/form/programa-form-fields";
import { ProgramaFormExitConfirmationDialog } from "../components/form/ui/programa-form-exit-confirmation-dialog";
import { showProgramaEducativoSavedToast } from "../components/form/ui/programa-educativo-form-notifications";
import { ProgramaFormSaveConfirmationDialog } from "../components/form/ui/programa-form-save-confirmation-dialog";
import type { ProgramaEducativo } from "../domain/programa-educativo";
import { useProgramaFormController } from "../presentation/hooks/use-programa-form-controller";
import { useUnsavedChangesWarning } from "../presentation/hooks/use-unsaved-changes-warning";
type Props = {
  programa?: ProgramaEducativo;
  readOnly?: boolean;
  title: string;
  description: string;
};
const PROGRAMA_FORM_ID = "programa-educativo-form";
export function ProgramaEducativoFormPage({
  programa,
  readOnly = false,
  title,
  description,
}: Props) {
  const router = useRouter();
  const {
    areas,
    areasQuery,
    entidades,
    entidadesQuery,
    errorFor,
    formValues,
    isDirty,
    isSaving,
    isSubmitDisabled,
    saveForm: submitForm,
    updateField,
    validateForm,
  } = useProgramaFormController({ programa });
  const [confirmationOpen, setConfirmationOpen] = useState(false);
  const [exitConfirmationOpen, setExitConfirmationOpen] = useState(false);
  useUnsavedChangesWarning(!readOnly && isDirty);
  function handleExit() {
    if (readOnly) {
      router.push("/ProgramasEducativos");
      return;
    }
    if (isDirty) {
      setExitConfirmationOpen(true);
      return;
    }
    router.push("/ProgramasEducativos");
  }
  async function saveForm() {
    setConfirmationOpen(false);
    const result = await submitForm();
    if (result.status !== "saved") return;
    router.push("/ProgramasEducativos");
    router.refresh();
    window.setTimeout(
      () => showProgramaEducativoSavedToast(result.operation),
      MINIMUM_LOADING_DURATION_MS,
    );
  }
  const catalogError = areasQuery.isError
    ? areasQuery.error
    : entidadesQuery.isError
      ? entidadesQuery.error
      : undefined;
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
                render={<Link href="/ProgramasEducativos" />}
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
                form={PROGRAMA_FORM_ID}
                disabled={
                  isSaving ||
                  areasQuery.isPending ||
                  entidadesQuery.isPending ||
                  isSubmitDisabled
                }
              >
                <Plus />
                {isSaving
                  ? "Guardando..."
                  : programa
                    ? "Guardar cambios"
                    : "Registrar programa"}
              </Button>
            ) : null}
          </>
        }
      />
      <Form
        id={PROGRAMA_FORM_ID}
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
          <ProgramaFormFields
            values={formValues}
            areas={areas}
            areasLoading={areasQuery.isPending}
            entidades={entidades.map((entidad) => ({
              value: entidad.idEntidadAcademica.toString(),
              label: entidad.nombre,
            }))}
            entidadesLoading={entidadesQuery.isPending}
            readOnly={readOnly}
            onChange={updateField}
            errorFor={errorFor}
          />
        </FormContent>
        {catalogError ? (
          <p className="text-sm font-medium text-destructive" role="alert">
            {getErrorMessage(catalogError)}
          </p>
        ) : null}
      </Form>
      {!readOnly ? (
        <>
          <ProgramaFormSaveConfirmationDialog
            isEditing={Boolean(programa)}
            open={confirmationOpen}
            onOpenChange={setConfirmationOpen}
            onConfirm={() => {
              void saveForm();
            }}
          />
          <ProgramaFormExitConfirmationDialog
            isEditing={Boolean(programa)}
            open={exitConfirmationOpen}
            onOpenChange={setExitConfirmationOpen}
            onConfirm={() => {
              setExitConfirmationOpen(false);
              router.push("/ProgramasEducativos");
            }}
          />
        </>
      ) : null}
    </>
  );
}

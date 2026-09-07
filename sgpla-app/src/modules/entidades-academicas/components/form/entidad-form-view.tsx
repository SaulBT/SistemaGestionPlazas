"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { Plus } from "lucide-react";
import { Form, FormContent } from "@/components/form";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";
import { getErrorMessage } from "@/shared/api/http-client";
import type { EntidadAcademica } from "../../domain/entidad-academica";
import { useEntidadForm } from "../../application/use-entidad-form";
import { EntidadFormFields } from "./entidad-form-fields";
import { EntidadFormExitConfirmationDialog } from "./ui/entidad-form-exit-confirmation-dialog";
import { EntidadFormSaveConfirmationDialog } from "./ui/entidad-form-save-confirmation-dialog";

type EntidadFormViewProps = {
  entidad?: EntidadAcademica;
  readOnly?: boolean;
  title: string;
  description: string;
};

const ENTIDAD_FORM_ID = "entidad-academica-form";

export function EntidadFormView({
  entidad,
  readOnly = false,
  title,
  description,
}: EntidadFormViewProps) {
  const router = useRouter();
  const {
    areas,
    areasQuery,
    errorFor,
    formError,
    formValues,
    isSubmitDisabled,
    isSaving,
    submitForm,
    updateField,
  } = useEntidadForm({ entidad });
  const [confirmationOpen, setConfirmationOpen] = useState(false);
  const [exitConfirmationOpen, setExitConfirmationOpen] = useState(false);
  const allowBrowserNavigationRef = useRef(false);
  const browserGuardActiveRef = useRef(false);
  const shouldConfirmExitRef = useRef(false);
  const hasFormValues = Object.values(formValues).some(
    (value) => value.trim().length > 0,
  );
  const shouldConfirmExit = Boolean(entidad) || hasFormValues;

  useEffect(() => {
    shouldConfirmExitRef.current = shouldConfirmExit;
  }, [shouldConfirmExit]);

  useEffect(() => {
    if (readOnly) return;

    function handlePopState() {
      if (!browserGuardActiveRef.current) return;

      if (allowBrowserNavigationRef.current) {
        allowBrowserNavigationRef.current = false;
        return;
      }

      if (!shouldConfirmExitRef.current) {
        allowBrowserNavigationRef.current = true;
        window.history.back();
        return;
      }

      const guardState = {
        ...(window.history.state ?? {}),
        entidadFormGuard: true,
      };

      window.history.pushState(guardState, "", window.location.href);
      setExitConfirmationOpen(true);
    }

    window.addEventListener("popstate", handlePopState);

    return () => window.removeEventListener("popstate", handlePopState);
  }, [readOnly]);

  useEffect(() => {
    if (readOnly) {
      return;
    }

    if (!shouldConfirmExit) {
      if (browserGuardActiveRef.current) {
        browserGuardActiveRef.current = false;
        allowBrowserNavigationRef.current = true;
        window.history.back();
      }

      return;
    }

    if (browserGuardActiveRef.current) return;

    const guardState = {
      ...(window.history.state ?? {}),
      entidadFormGuard: true,
    };

    window.history.pushState(guardState, "", window.location.href);
    browserGuardActiveRef.current = true;
  }, [readOnly, shouldConfirmExit]);

  function handleExit() {
    if (readOnly) {
      router.push("/EntidadesAcademicas");
      return;
    }

    if (shouldConfirmExit) {
      setExitConfirmationOpen(true);
      return;
    }

    router.push("/EntidadesAcademicas");
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
                setConfirmationOpen(true);
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
              setConfirmationOpen(false);
              void submitForm();
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

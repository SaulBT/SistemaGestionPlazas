"use client";

import { useState } from "react";
import {
  HttpClientError,
  type ValidationErrors,
} from "@/shared/api/http-client";
import { toZodValidationErrors } from "@/shared/forms/zod-validation-errors";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import type { Articulo } from "../domain/articulo";
import {
  articuloFormSchema,
  type ArticuloFormValues,
} from "../application/articulo-form.validation";
import { useGuardarArticulo } from "../presentation/articulos.queries";

type Props = {
  articulo?: Articulo;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSaved: (editing: boolean) => void;
};

export function ArticuloFormDialog({
  articulo,
  open,
  onOpenChange,
  onSaved,
}: Props) {
  const editing = Boolean(articulo);
  const [values, setValues] = useState<ArticuloFormValues>(() => ({
    numero: articulo?.numero ?? "",
    descripcion: articulo?.descripcion ?? "",
  }));
  const [errors, setErrors] = useState<ValidationErrors>({});
  const mutation = useGuardarArticulo();

  const errorFor = (field: keyof ArticuloFormValues) => errors[field]?.[0];

  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const result = articuloFormSchema.safeParse(values);
    if (!result.success) {
      setErrors(toZodValidationErrors(result.error));
      return;
    }
    try {
      await mutation.mutateAsync({
        input: result.data,
        articuloExistente: articulo,
      });
      onOpenChange(false);
      onSaved(editing);
    } catch (error) {
      if (error instanceof HttpClientError)
        setErrors(
          error.validationErrors ??
            (error.status === 409 ? { numero: [error.message] } : {}),
        );
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {editing ? "Editar artículo" : "Registrar artículo"}
          </DialogTitle>
          <DialogDescription>
            {editing
              ? "Actualiza los datos del artículo."
              : "Registra un nuevo artículo para el sistema."}
          </DialogDescription>
        </DialogHeader>
        <form id="articulo-form" className="grid gap-4" onSubmit={submit}>
          <Field>
            <FieldLabel htmlFor="numero-articulo">
              Número del artículo
            </FieldLabel>
            <Input
              id="numero-articulo"
              value={values.numero}
              maxLength={50}
              onChange={(event) =>
                setValues((current) => ({
                  ...current,
                  numero: event.target.value,
                }))
              }
              aria-invalid={Boolean(errorFor("numero"))}
              placeholder="Ej. Artículo 1"
            />
            <FieldError>{errorFor("numero")}</FieldError>
          </Field>
          <Field>
            <FieldLabel htmlFor="descripcion-articulo">Descripción</FieldLabel>
            <Textarea
              id="descripcion-articulo"
              value={values.descripcion}
              onChange={(event) =>
                setValues((current) => ({
                  ...current,
                  descripcion: event.target.value,
                }))
              }
              aria-invalid={Boolean(errorFor("descripcion"))}
              placeholder="Describe el artículo..."
            />
            <FieldError>{errorFor("descripcion")}</FieldError>
          </Field>
        </form>
        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
          >
            Cancelar
          </Button>
          <Button
            type="submit"
            form="articulo-form"
            disabled={mutation.isPending}
          >
            {mutation.isPending
              ? "Guardando..."
              : editing
                ? "Guardar cambios"
                : "Registrar artículo"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

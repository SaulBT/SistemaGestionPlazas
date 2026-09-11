"use client";

import { ConfirmationDialog } from "@/components/ui/confirmation-dialog";

type Props = {
  isEditing: boolean;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
};

export function EntidadFormExitConfirmationDialog({
  isEditing,
  open,
  onOpenChange,
  onConfirm,
}: Props) {
  return (
    <ConfirmationDialog
      open={open}
      title={isEditing ? "¿Salir de la edición?" : "¿Salir del registro?"}
      description={
        isEditing
          ? "Los cambios realizados no se guardarán."
          : "La información capturada se perderá."
      }
      confirmLabel="Salir sin guardar"
      cancelLabel="Continuar aquí"
      destructive
      onOpenChange={onOpenChange}
      onConfirm={onConfirm}
    />
  );
}

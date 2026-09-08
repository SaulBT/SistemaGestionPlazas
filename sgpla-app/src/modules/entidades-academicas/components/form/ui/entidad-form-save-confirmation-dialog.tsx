"use client";

import { ConfirmationDialog } from "@/components/ui/confirmation-dialog";

type Props = {
  isEditing: boolean;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
};

export function EntidadFormSaveConfirmationDialog({
  isEditing,
  open,
  onOpenChange,
  onConfirm,
}: Props) {
  return (
    <ConfirmationDialog
      open={open}
      title={
        isEditing
          ? "¿Guardar cambios de la entidad?"
          : "¿Registrar esta entidad académica?"
      }
      description={
        isEditing
          ? "Se actualizará la información de la entidad académica."
          : "Se registrará una nueva entidad académica con los datos capturados."
      }
      confirmLabel={isEditing ? "Guardar cambios" : "Registrar entidad"}
      onOpenChange={onOpenChange}
      onConfirm={onConfirm}
    />
  );
}

"use client";

import { ConfirmationDialog } from "@/components/ui/confirmation-dialog";

type Props = {
  isEditing: boolean;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
};

export function DireccionFormSaveConfirmationDialog({
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
          ? "¿Guardar cambios de la dirección?"
          : "¿Registrar esta dirección de área académica?"
      }
      description={
        isEditing
          ? "Se actualizará la información de la dirección de área académica."
          : "Se registrará una nueva dirección con los datos capturados."
      }
      confirmLabel={isEditing ? "Guardar cambios" : "Registrar dirección"}
      onOpenChange={onOpenChange}
      onConfirm={onConfirm}
    />
  );
}
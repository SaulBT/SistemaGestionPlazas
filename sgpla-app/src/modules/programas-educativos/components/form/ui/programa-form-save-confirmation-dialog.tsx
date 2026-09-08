"use client";
import { ConfirmationDialog } from "@/components/ui/confirmation-dialog";
type Props = {
  isEditing: boolean;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
};
export function ProgramaFormSaveConfirmationDialog({
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
          ? "¿Guardar cambios del programa?"
          : "¿Registrar este programa educativo?"
      }
      description={
        isEditing
          ? "Se actualizará la información del programa educativo."
          : "Se registrará un nuevo programa educativo con los datos capturados."
      }
      confirmLabel={isEditing ? "Guardar cambios" : "Registrar programa"}
      onOpenChange={onOpenChange}
      onConfirm={onConfirm}
    />
  );
}

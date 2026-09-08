"use client";

import { ConfirmationDialog } from "@/components/ui/confirmation-dialog";

type Props = {
  direccionNombre?: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
};

export function DeleteDireccionAreaAcademicaDialog({
  direccionNombre,
  open,
  onOpenChange,
  onConfirm,
}: Props) {
  return (
    <ConfirmationDialog
      open={open}
      title="¿Eliminar dirección de área académica?"
      description={
        direccionNombre
          ? `La dirección “${direccionNombre}” se eliminará permanentemente.`
          : "Esta acción no se puede deshacer."
      }
      confirmLabel="Eliminar dirección"
      destructive
      onOpenChange={onOpenChange}
      onConfirm={onConfirm}
    />
  );
}
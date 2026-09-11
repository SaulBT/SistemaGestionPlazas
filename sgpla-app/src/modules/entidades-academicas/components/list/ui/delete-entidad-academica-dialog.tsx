"use client";

import { ConfirmationDialog } from "@/components/ui/confirmation-dialog";

type Props = {
  entidadNombre?: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
};

export function DeleteEntidadAcademicaDialog({
  entidadNombre,
  open,
  onOpenChange,
  onConfirm,
}: Props) {
  return (
    <ConfirmationDialog
      open={open}
      title="¿Eliminar entidad académica?"
      description={
        entidadNombre
          ? `La entidad “${entidadNombre}” se eliminará permanentemente.`
          : "Esta acción no se puede deshacer."
      }
      confirmLabel="Eliminar entidad"
      destructive
      onOpenChange={onOpenChange}
      onConfirm={onConfirm}
    />
  );
}

"use client";
import { ConfirmationDialog } from "@/components/ui/confirmation-dialog";
type Props = {
  programaNombre?: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
};
export function DeleteProgramaEducativoDialog({
  programaNombre,
  open,
  onOpenChange,
  onConfirm,
}: Props) {
  return (
    <ConfirmationDialog
      open={open}
      title="¿Eliminar programa educativo?"
      description={
        programaNombre
          ? `El programa “${programaNombre}” se eliminará permanentemente.`
          : "Esta acción no se puede deshacer."
      }
      confirmLabel="Eliminar programa"
      destructive
      onOpenChange={onOpenChange}
      onConfirm={onConfirm}
    />
  );
}

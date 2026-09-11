import { toast } from "@/components/ui/toast";

export function showDireccionAreaAcademicaDeletionToast(operation: Promise<unknown>) {
  toast.promise(operation, {
    loading: "Eliminando dirección de área académica...",
    success: "Dirección de área académica eliminada correctamente.",
    error: "No se pudo eliminar la dirección de área académica.",
  });
}
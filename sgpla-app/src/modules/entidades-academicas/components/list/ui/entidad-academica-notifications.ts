import { toast } from "@/components/ui/toast";

export function showEntidadAcademicaDeletionToast(operation: Promise<unknown>) {
  toast.promise(operation, {
    loading: "Eliminando entidad académica...",
    success: "Entidad académica eliminada correctamente.",
    error: "No se pudo eliminar la entidad académica.",
  });
}

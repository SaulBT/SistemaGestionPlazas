import { toast } from "@/components/ui/toast";
export function showProgramaEducativoDeletionToast(
  operation: Promise<unknown>,
) {
  toast.promise(operation, {
    loading: "Eliminando programa educativo...",
    success: "Programa educativo eliminado correctamente.",
    error: "No se pudo eliminar el programa educativo.",
  });
}

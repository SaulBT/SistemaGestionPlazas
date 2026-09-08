import { toast } from "@/components/ui/toast";

export function showEntidadAcademicaSavedToast(
  operation: "created" | "updated",
) {
  const isUpdate = operation === "updated";

  toast.add({
    type: "success",
    title: isUpdate
      ? "Entidad académica actualizada"
      : "Entidad académica registrada",
    description: isUpdate
      ? "Los cambios se guardaron correctamente."
      : "La entidad académica se registró correctamente.",
  });
}

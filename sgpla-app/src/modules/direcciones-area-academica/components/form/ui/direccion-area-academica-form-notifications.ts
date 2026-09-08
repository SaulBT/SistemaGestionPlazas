import { toast } from "@/components/ui/toast";

export function showDireccionAreaAcademicaSavedToast(
  operation: "created" | "updated",
) {
  const isUpdate = operation === "updated";

  toast.add({
    type: "success",
    title: isUpdate
      ? "Dirección de área académica actualizada"
      : "Dirección de área académica registrada",
    description: isUpdate
      ? "Los cambios se guardaron correctamente."
      : "La dirección de área académica se registró correctamente.",
  });
}
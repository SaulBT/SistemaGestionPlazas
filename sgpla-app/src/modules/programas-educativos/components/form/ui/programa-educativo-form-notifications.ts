import { toast } from "@/components/ui/toast";

export function showProgramaEducativoSavedToast(
  operation: "created" | "updated",
) {
  const isUpdate = operation === "updated";
  toast.add({
    type: "success",
    title: isUpdate
      ? "Programa educativo actualizado"
      : "Programa educativo registrado",
    description: isUpdate
      ? "Los cambios se guardaron correctamente."
      : "El programa educativo se registró correctamente.",
  });
}

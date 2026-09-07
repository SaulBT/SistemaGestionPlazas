import { Dashboard } from "@/components/dashboard";
import { EntidadFormView } from "@/modules/entidades-academicas";
export const metadata = { title: "Agregar Entidad Académica - SGPla" };
export default function CrearEntidadPage() {
  return (
    <Dashboard activeHref="/EntidadesAcademicas">
      <EntidadFormView
        title="Agregar Entidad Académica"
        description="Registra una nueva entidad académica."
      />
    </Dashboard>
  );
}

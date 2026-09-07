import { Dashboard } from "@/components/dashboard";
import { EntidadAcademicaFormPage } from "@/modules/entidades-academicas";
export const metadata = { title: "Agregar Entidad Académica - SGPla" };
export default function CrearEntidadPage() {
  return (
    <Dashboard activeHref="/EntidadesAcademicas">
      <EntidadAcademicaFormPage
        title="Agregar Entidad Académica"
        description="Registra una nueva entidad académica."
      />
    </Dashboard>
  );
}

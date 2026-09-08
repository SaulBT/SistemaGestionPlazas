import { Dashboard } from "@/components/dashboard";
import { DireccionAreaAcademicaFormPage } from "@/modules/direcciones-area-academica";

export const metadata = {
  title: "Agregar Dirección de Área Académica - SGPla",
};

export default function CrearDireccionPage() {
  return (
    <Dashboard activeHref="/DireccionesAreaAcademica">
      <DireccionAreaAcademicaFormPage
        title="Agregar Dirección de Área Académica"
        description="Registra una nueva dirección de área académica."
      />
    </Dashboard>
  );
}
import { Dashboard } from "@/components/dashboard";
import { DireccionesAreaAcademicaListPage } from "@/modules/direcciones-area-academica";

export const metadata = { title: "Direcciones de Áreas Académicas - SGPla" };

export default function DireccionesPage() {
  return (
    <Dashboard activeHref="/DireccionesAreaAcademica">
      <DireccionesAreaAcademicaListPage />
    </Dashboard>
  );
}
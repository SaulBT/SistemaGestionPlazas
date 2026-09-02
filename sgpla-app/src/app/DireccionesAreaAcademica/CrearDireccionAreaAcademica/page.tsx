import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { DireccionFormView } from "@/modules/direcciones-area-academica";

export const metadata = { title: "Agregar Dirección de Área Académica - SGPla" };

export default function CrearDireccionPage() {
  return (
    <Dashboard activeHref="/DireccionesAreaAcademica">
      <PageHeader title="Agregar Dirección de Área Académica" description="Registra una nueva dirección de área académica." />
      <DireccionFormView />
    </Dashboard>
  );
}

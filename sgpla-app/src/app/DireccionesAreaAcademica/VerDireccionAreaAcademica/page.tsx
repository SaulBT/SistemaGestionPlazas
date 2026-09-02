import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { DireccionFormView } from "@/modules/direcciones-area-academica";
import { direccionesAreaAcademica } from "@/modules/direcciones-area-academica/data";

export const metadata = { title: "Dirección de Área Académica - SGPla" };

export default async function VerDireccionPage({ searchParams }: { searchParams: Promise<{ id?: string }> }) {
  const { id } = await searchParams;
  const direccion = direccionesAreaAcademica.find((item) => item.id === id);

  return (
    <Dashboard activeHref="/DireccionesAreaAcademica">
      <PageHeader title="Dirección de Área Académica" description="Consulta la información de la dirección." />
      <DireccionFormView direccion={direccion} readOnly />
    </Dashboard>
  );
}

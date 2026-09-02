import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { DireccionFormView } from "@/modules/direcciones-area-academica";
import { direccionesAreaAcademica } from "@/modules/direcciones-area-academica/data";

export const metadata = { title: "Editar Dirección de Área Académica - SGPla" };

export default async function EditarDireccionPage({ searchParams }: { searchParams: Promise<{ id?: string }> }) {
  const { id } = await searchParams;
  const direccion = direccionesAreaAcademica.find((item) => item.id === id);

  return (
    <Dashboard activeHref="/DireccionesAreaAcademica">
      <PageHeader title="Editar Dirección de Área Académica" description="Actualiza la información de la dirección." />
      <DireccionFormView direccion={direccion} />
    </Dashboard>
  );
}

import { Dashboard } from "@/components/dashboard";
import { DireccionAreaAcademicaFormPageLoader } from "@/modules/direcciones-area-academica";

export const metadata = { title: "Dirección de Área Académica - SGPla" };

export default async function VerDireccionPage({
  searchParams,
}: {
  searchParams: Promise<{ id?: string }>;
}) {
  const { id } = await searchParams;

  return (
    <Dashboard activeHref="/DireccionesAreaAcademica">
      <DireccionAreaAcademicaFormPageLoader
        idAreaAcademica={Number(id)}
        title="Dirección de Área Académica"
        description="Consulta la información de la dirección."
        readOnly
      />
    </Dashboard>
  );
}
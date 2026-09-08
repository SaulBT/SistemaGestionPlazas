import { Dashboard } from "@/components/dashboard";
import { DireccionAreaAcademicaFormPageLoader } from "@/modules/direcciones-area-academica";

export const metadata = {
  title: "Editar Dirección de Área Académica - SGPla",
};

export default async function EditarDireccionPage({
  searchParams,
}: {
  searchParams: Promise<{ id?: string }>;
}) {
  const { id } = await searchParams;

  return (
    <Dashboard activeHref="/DireccionesAreaAcademica">
      <DireccionAreaAcademicaFormPageLoader
        idAreaAcademica={Number(id)}
        title="Editar Dirección de Área Académica"
        description="Actualiza la información de la dirección."
      />
    </Dashboard>
  );
}
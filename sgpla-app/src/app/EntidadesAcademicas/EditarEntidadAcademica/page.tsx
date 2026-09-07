import { Dashboard } from "@/components/dashboard";
import { EntidadAcademicaFormPageLoader } from "@/modules/entidades-academicas";
export const metadata = { title: "Editar Entidad Académica - SGPla" };
export default async function EditarEntidadPage({
  searchParams,
}: {
  searchParams: Promise<{ id?: string }>;
}) {
  const { id } = await searchParams;
  return (
    <Dashboard activeHref="/EntidadesAcademicas">
      <EntidadAcademicaFormPageLoader
        idEntidadAcademica={Number(id)}
        title="Editar Entidad Académica"
        description="Actualiza la información de la entidad."
      />
    </Dashboard>
  );
}

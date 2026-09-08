import { Dashboard } from "@/components/dashboard";
import { EntidadAcademicaFormPageLoader } from "@/modules/entidades-academicas";
export const metadata = { title: "Entidad Académica - SGPla" };
export default async function VerEntidadPage({
  searchParams,
}: {
  searchParams: Promise<{ id?: string }>;
}) {
  const { id } = await searchParams;
  return (
    <Dashboard activeHref="/EntidadesAcademicas">
      <EntidadAcademicaFormPageLoader
        idEntidadAcademica={Number(id)}
        title="Entidad Académica"
        description="Consulta la información de la entidad."
        readOnly
      />
    </Dashboard>
  );
}

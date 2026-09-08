import { Dashboard } from "@/components/dashboard";
import { ProgramaEducativoFormPageLoader } from "@/modules/programas-educativos";
export const metadata = { title: "Editar Programa Educativo - SGPla" };
export default async function EditarProgramaPage({
  searchParams,
}: {
  searchParams: Promise<{ id?: string }>;
}) {
  const { id } = await searchParams;
  return (
    <Dashboard activeHref="/ProgramasEducativos">
      <ProgramaEducativoFormPageLoader
        idProgramaEducativo={Number(id)}
        title="Editar Programa Educativo"
        description="Actualiza la información del programa educativo."
      />
    </Dashboard>
  );
}

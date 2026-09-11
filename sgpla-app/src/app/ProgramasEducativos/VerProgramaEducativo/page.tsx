import { Dashboard } from "@/components/dashboard";
import { ProgramaEducativoFormPageLoader } from "@/modules/programas-educativos";
export const metadata = { title: "Programa Educativo - SGPla" };
export default async function VerProgramaPage({
  searchParams,
}: {
  searchParams: Promise<{ id?: string }>;
}) {
  const { id } = await searchParams;
  return (
    <Dashboard activeHref="/ProgramasEducativos">
      <ProgramaEducativoFormPageLoader
        idProgramaEducativo={Number(id)}
        title="Programa Educativo"
        description="Consulta la información del programa educativo."
        readOnly
      />
    </Dashboard>
  );
}

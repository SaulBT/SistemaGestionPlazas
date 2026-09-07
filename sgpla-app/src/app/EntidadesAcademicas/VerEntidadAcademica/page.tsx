import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { EntidadFormLoader } from "@/modules/entidades-academicas";
export const metadata = { title: "Entidad Académica - SGPla" };
export default async function VerEntidadPage({ searchParams }: { searchParams: Promise<{ id?: string }> }) { const { id } = await searchParams; return <Dashboard activeHref="/EntidadesAcademicas"><PageHeader title="Entidad Académica" description="Consulta la información de la entidad." /><EntidadFormLoader idEntidadAcademica={Number(id)} readOnly /></Dashboard>; }

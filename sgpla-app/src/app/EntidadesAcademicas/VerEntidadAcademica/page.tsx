import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { EntidadFormView } from "@/modules/entidades-academicas";
import { entidadesAcademicas } from "@/modules/entidades-academicas/data";
export const metadata = { title: "Entidad Académica - SGPla" };
export default async function VerEntidadPage({ searchParams }: { searchParams: Promise<{ id?: string }> }) { const { id } = await searchParams; const entidad = entidadesAcademicas.find((item) => item.id === id); return <Dashboard activeHref="/EntidadesAcademicas"><PageHeader title="Entidad Académica" description="Consulta la información de la entidad." /><EntidadFormView entidad={entidad} readOnly /></Dashboard>; }

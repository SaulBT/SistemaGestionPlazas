import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { EntidadFormLoader } from "@/modules/entidades-academicas";
export const metadata = { title: "Editar Entidad Académica - SGPla" };
export default async function EditarEntidadPage({ searchParams }: { searchParams: Promise<{ id?: string }> }) { const { id } = await searchParams; return <Dashboard activeHref="/EntidadesAcademicas"><PageHeader title="Editar Entidad Académica" description="Actualiza la información de la entidad." /><EntidadFormLoader idEntidadAcademica={Number(id)} /></Dashboard>; }

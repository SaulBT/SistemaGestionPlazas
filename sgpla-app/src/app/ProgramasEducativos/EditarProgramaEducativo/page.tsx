import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { ProgramaFormView } from "@/modules/programas-educativos";
import { programasEducativos } from "@/modules/programas-educativos/data";
export const metadata = { title: "Editar Programa Educativo - SGPla" };
export default async function EditarProgramaPage({ searchParams }: { searchParams: Promise<{ id?: string }> }) { const { id } = await searchParams; const programa = programasEducativos.find((item) => item.id === id); return <Dashboard activeHref="/ProgramasEducativos"><PageHeader title="Editar Programa Educativo" description="Actualiza la información del programa educativo." /><ProgramaFormView programa={programa} /></Dashboard>; }

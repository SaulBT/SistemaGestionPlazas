import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { ProgramaFormView } from "@/modules/programas-educativos";
import { programasEducativos } from "@/modules/programas-educativos/data";
export const metadata = { title: "Programa Educativo - SGPla" };
export default async function VerProgramaPage({ searchParams }: { searchParams: Promise<{ id?: string }> }) { const { id } = await searchParams; const programa = programasEducativos.find((item) => item.id === id); return <Dashboard activeHref="/ProgramasEducativos"><PageHeader title="Programa Educativo" description="Consulta la información del programa educativo." /><ProgramaFormView programa={programa} readOnly /></Dashboard>; }

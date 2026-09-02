import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { ProgramaFormView } from "@/modules/programas-educativos";
export const metadata = { title: "Crear Programa Educativo - SGPla" };
export default function CrearProgramaPage() { return <Dashboard activeHref="/ProgramasEducativos"><PageHeader title="Crear Programa Educativo" description="Registra un nuevo programa educativo." /><ProgramaFormView /></Dashboard>; }

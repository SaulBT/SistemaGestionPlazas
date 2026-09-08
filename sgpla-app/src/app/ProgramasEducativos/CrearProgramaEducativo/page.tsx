import { Dashboard } from "@/components/dashboard";
import { ProgramaEducativoFormPage } from "@/modules/programas-educativos";
export const metadata = { title: "Crear Programa Educativo - SGPla" };
export default function CrearProgramaPage() {
  return (
    <Dashboard activeHref="/ProgramasEducativos">
      <ProgramaEducativoFormPage
        title="Crear Programa Educativo"
        description="Registra un nuevo programa educativo."
      />
    </Dashboard>
  );
}

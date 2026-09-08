import { Dashboard } from "@/components/dashboard";
import { ProgramasEducativosListPage } from "@/modules/programas-educativos";
export const metadata = { title: "Programas Educativos - SGPla" };
export default function ProgramasPage() {
  return (
    <Dashboard activeHref="/ProgramasEducativos">
      <ProgramasEducativosListPage />
    </Dashboard>
  );
}

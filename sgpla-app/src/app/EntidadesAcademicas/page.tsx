import { EntidadesAcademicasListPage } from "@/modules/entidades-academicas";
import { Dashboard } from "@/components/dashboard";
export const metadata = { title: "Entidades Académicas - SGPla" };
export default function EntidadesPage() {
  return (
    <Dashboard activeHref="/EntidadesAcademicas">
      <EntidadesAcademicasListPage />
    </Dashboard>
  );
}

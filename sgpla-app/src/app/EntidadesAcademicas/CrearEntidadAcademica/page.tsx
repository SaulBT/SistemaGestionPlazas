import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { EntidadFormView } from "@/modules/entidades-academicas";
export const metadata = { title: "Agregar Entidad Académica - SGPla" };
export default function CrearEntidadPage() { return <Dashboard activeHref="/EntidadesAcademicas"><PageHeader title="Agregar Entidad Académica" description="Registra una nueva entidad académica." /><EntidadFormView /></Dashboard>; }

import Link from "next/link";
import { Plus } from "lucide-react";
import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";
import { EntidadesDataGrid } from "./entidades-data-grid";
import { EntidadesFiltersHeader } from "./entidades-filters-header";

export function EntidadesListView() {
  return <Dashboard activeHref="/EntidadesAcademicas"><div className="mx-auto max-w-7xl space-y-6 pt-24"><PageHeader title="Entidades Académicas" description="Administra las entidades académicas y su información de contacto." actions={<Button nativeButton={false} render={<Link href="/EntidadesAcademicas/CrearEntidadAcademica" />}><Plus />Crear Entidad Académica</Button>} /><EntidadesFiltersHeader /><EntidadesDataGrid /><nav aria-label="Paginación de entidades académicas" className="flex justify-end text-sm text-muted-foreground">Página 1 de 1</nav></div></Dashboard>;
}

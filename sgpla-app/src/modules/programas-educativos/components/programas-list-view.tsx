import Link from "next/link";
import { Plus } from "lucide-react";
import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";
import { ProgramasDataGrid } from "./programas-data-grid";
import { ProgramasFiltersHeader } from "./programas-filters-header";

export function ProgramasListView() { return <Dashboard activeHref="/ProgramasEducativos"><div className="mx-auto max-w-7xl space-y-6 pt-24"><PageHeader title="Programas Educativos" description="Administra los programas educativos de las entidades académicas." actions={<Button nativeButton={false} render={<Link href="/ProgramasEducativos/CrearProgramaEducativo" />}><Plus />Crear Programa Educativo</Button>} /><ProgramasFiltersHeader /><ProgramasDataGrid /><nav aria-label="Paginación de programas educativos" className="flex justify-end text-sm text-muted-foreground">Página 1 de 1</nav></div></Dashboard>; }

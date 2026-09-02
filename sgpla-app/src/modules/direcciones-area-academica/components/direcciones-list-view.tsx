import Link from "next/link";
import { Plus } from "lucide-react";

import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";

import { DireccionesDataGrid } from "./direcciones-data-grid";
import { DireccionesFiltersHeader } from "./direcciones-filters-header";

export function DireccionesListView() {
  return (
    <Dashboard activeHref="/DireccionesAreaAcademica">
      <div className="mx-auto max-w-7xl space-y-6 pt-24">
        <PageHeader
          title="Direcciones de Áreas Académicas"
          description="Administra las direcciones de las áreas académicas."
          actions={
            <Button nativeButton={false} render={<Link href="/DireccionesAreaAcademica/CrearDireccionAreaAcademica" />}>
              <Plus />
              Agregar Dirección
            </Button>
          }
        />
        <DireccionesFiltersHeader />
        <DireccionesDataGrid />
        <nav aria-label="Paginación de direcciones" className="flex justify-end text-sm text-muted-foreground">
          Página 1 de 1
        </nav>
      </div>
    </Dashboard>
  );
}

import Link from "next/link";
import { Plus } from "lucide-react";

import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";
import { UsersDataGrid } from "./users-data-grid";
import { UsersFiltersHeader } from "./users-filters-header";
import { UsersPagination } from "./users-pagination";

export function UsersListView() {
  return (
    <Dashboard activeHref="/Usuarios">
      <div className="mx-auto max-w-7xl space-y-6 pt-24">
        <PageHeader
          title="Usuarios"
          description="Administra los usuarios y sus permisos dentro del sistema."
          actions={
            <Button
              nativeButton={false}
              render={<Link href="/Usuarios/CrearUsuario" />}
            >
              <Plus />
              Crear Usuario
            </Button>
          }
        />
        <UsersFiltersHeader />
        <UsersDataGrid />
        <UsersPagination />
      </div>
    </Dashboard>
  );
}

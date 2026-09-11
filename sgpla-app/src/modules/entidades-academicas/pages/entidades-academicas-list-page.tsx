"use client";

import { useCallback } from "react";
import Link from "next/link";
import { Plus } from "lucide-react";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";
import { getErrorMessage } from "@/shared/api/http-client";
import type { EntidadAcademica } from "../domain/entidad-academica";
import { useEntidadesAcademicasListController } from "../presentation/hooks/use-entidades-academicas-list-controller";
import { EntidadesDataGrid } from "../components/list/entidades-data-grid";
import { EntidadesFiltersHeader } from "../components/list/entidades-filters-header";
import { DeleteEntidadAcademicaDialog } from "../components/list/ui/delete-entidad-academica-dialog";
import { showEntidadAcademicaDeletionToast } from "../components/list/ui/entidad-academica-notifications";
import { EntidadesPagination } from "../components/list/ui/entidades-pagination";

export function EntidadesAcademicasListPage() {
  const controller = useEntidadesAcademicasListController();
  const getViewHref = useCallback(
    (entidad: EntidadAcademica) =>
      `/EntidadesAcademicas/VerEntidadAcademica?id=${entidad.idEntidadAcademica}`,
    [],
  );
  const getEditHref = useCallback(
    (entidad: EntidadAcademica) =>
      `/EntidadesAcademicas/EditarEntidadAcademica?id=${entidad.idEntidadAcademica}`,
    [],
  );
  const getProgramsHref = useCallback(
    (entidad: EntidadAcademica) =>
      `/ProgramasEducativos/Buscar?idEntidadAcademica=${entidad.idEntidadAcademica}&region=${encodeURIComponent(entidad.region)}&idAreaAcademica=${entidad.idAreaAcademica}`,
    [],
  );

  function confirmDeletion() {
    const operation = controller.confirmDeletion();
    showEntidadAcademicaDeletionToast(operation);
    void operation.catch(() => undefined);
  }

  return (
    <>
      <div className="mx-auto max-w-7xl space-y-6 pt-24">
        <PageHeader
          title="Entidades Académicas"
          description="Administra las entidades académicas y su información de contacto."
          actions={
            <Button
              nativeButton={false}
              render={
                <Link href="/EntidadesAcademicas/CrearEntidadAcademica" />
              }
            >
              <Plus />
              Crear Entidad Académica
            </Button>
          }
        />

        <EntidadesFiltersHeader
          consulta={controller.query}
          areas={controller.areas}
          areasLoading={controller.areasQuery.isPending}
          onChange={controller.updateFilters}
        />

        {controller.entidadesQuery.isError || controller.areasQuery.isError ? (
          <p className="text-sm font-medium text-destructive" role="alert">
            {getErrorMessage(
              controller.entidadesQuery.isError
                ? controller.entidadesQuery.error
                : controller.areasQuery.error,
            )}
          </p>
        ) : null}

        <EntidadesDataGrid
          entidades={controller.page?.items ?? []}
          isLoading={controller.entidadesQuery.isPending}
          isDeleting={controller.isDeleting}
          createHref="/EntidadesAcademicas/CrearEntidadAcademica"
          getViewHref={getViewHref}
          getEditHref={getEditHref}
          getProgramsHref={getProgramsHref}
          onDelete={controller.requestDeletion}
        />

        <EntidadesPagination
          currentPage={controller.query.pagina}
          totalPages={controller.totalPages}
          isLoading={controller.entidadesQuery.isPending}
          onPageChange={controller.changePage}
        />
      </div>

      <DeleteEntidadAcademicaDialog
        entidadNombre={controller.pendingDeletion?.nombre}
        open={Boolean(controller.pendingDeletion)}
        onOpenChange={controller.setDeletionDialogOpen}
        onConfirm={confirmDeletion}
      />
    </>
  );
}

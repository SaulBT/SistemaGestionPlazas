"use client";

import { useCallback } from "react";
import Link from "next/link";
import { Plus } from "lucide-react";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";
import { getErrorMessage } from "@/shared/api/http-client";
import { DireccionesDataGrid } from "../components/list/direcciones-data-grid";
import { DireccionesFiltersHeader } from "../components/list/direcciones-filters-header";
import { DeleteDireccionAreaAcademicaDialog } from "../components/list/ui/delete-direccion-area-academica-dialog";
import { showDireccionAreaAcademicaDeletionToast } from "../components/list/ui/direccion-area-academica-notifications";
import { DireccionesPagination } from "../components/list/ui/direcciones-pagination";
import type { DireccionAreaAcademica } from "../domain/direccion-area-academica";
import { useDireccionesAreaAcademicaListController } from "../presentation/hooks/use-direcciones-area-academica-list-controller";

export function DireccionesAreaAcademicaListPage() {
  const controller = useDireccionesAreaAcademicaListController();
  const getViewHref = useCallback(
    (direccion: DireccionAreaAcademica) =>
      `/DireccionesAreaAcademica/VerDireccionAreaAcademica?id=${direccion.idAreaAcademica}`,
    [],
  );
  const getEditHref = useCallback(
    (direccion: DireccionAreaAcademica) =>
      `/DireccionesAreaAcademica/EditarDireccionAreaAcademica?id=${direccion.idAreaAcademica}`,
    [],
  );

  function confirmDeletion() {
    const operation = controller.confirmDeletion();
    showDireccionAreaAcademicaDeletionToast(operation);
    void operation.catch(() => undefined);
  }

  return (
    <>
      <div className="mx-auto max-w-7xl space-y-6 pt-24">
        <PageHeader
          title="Direcciones de Áreas Académicas"
          description="Administra las direcciones de las áreas académicas."
          actions={
            <Button
              nativeButton={false}
              render={
                <Link href="/DireccionesAreaAcademica/CrearDireccionAreaAcademica" />
              }
            >
              <Plus />
              Crear Dirección de Área Académica
            </Button>
          }
        />

        <DireccionesFiltersHeader
          consulta={controller.query}
          onChange={controller.updateFilters}
        />

        {controller.direccionesQuery.isError ? (
          <p className="text-sm font-medium text-destructive" role="alert">
            {getErrorMessage(controller.direccionesQuery.error)}
          </p>
        ) : null}

        <DireccionesDataGrid
          direcciones={controller.page?.items ?? []}
          isLoading={controller.direccionesQuery.isPending}
          isDeleting={controller.isDeleting}
          createHref="/DireccionesAreaAcademica/CrearDireccionAreaAcademica"
          getViewHref={getViewHref}
          getEditHref={getEditHref}
          onDelete={controller.requestDeletion}
        />

        <DireccionesPagination
          currentPage={controller.query.pagina}
          totalPages={controller.totalPages}
          isLoading={controller.direccionesQuery.isPending}
          onPageChange={controller.changePage}
        />
      </div>

      <DeleteDireccionAreaAcademicaDialog
        direccionNombre={controller.pendingDeletion?.nombre}
        open={Boolean(controller.pendingDeletion)}
        onOpenChange={controller.setDeletionDialogOpen}
        onConfirm={confirmDeletion}
      />
    </>
  );
}
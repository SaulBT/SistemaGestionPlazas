"use client";
import { useCallback } from "react";
import Link from "next/link";
import { Plus } from "lucide-react";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";
import { getErrorMessage } from "@/shared/api/http-client";
import { ProgramasDataGrid } from "../components/list/programas-data-grid";
import { ProgramasFiltersHeader } from "../components/list/programas-filters-header";
import { DeleteProgramaEducativoDialog } from "../components/list/ui/delete-programa-educativo-dialog";
import { showProgramaEducativoDeletionToast } from "../components/list/ui/programa-educativo-notifications";
import { ProgramasPagination } from "../components/list/ui/programas-pagination";
import type { ProgramaEducativo } from "../domain/programa-educativo";
import { useProgramasEducativosListController } from "../presentation/hooks/use-programas-educativos-list-controller";
export function ProgramasEducativosListPage() {
  const controller = useProgramasEducativosListController();
  const getViewHref = useCallback(
    (programa: ProgramaEducativo) =>
      `/ProgramasEducativos/VerProgramaEducativo?id=${programa.idProgramaEducativo}`,
    [],
  );
  const getEditHref = useCallback(
    (programa: ProgramaEducativo) =>
      `/ProgramasEducativos/EditarProgramaEducativo?id=${programa.idProgramaEducativo}`,
    [],
  );
  const getPlansHref = useCallback(
    (programa: ProgramaEducativo) =>
      `/PlanesEstudios?programaEducativoId=${programa.idProgramaEducativo}`,
    [],
  );
  function confirmDeletion() {
    const operation = controller.confirmDeletion();
    showProgramaEducativoDeletionToast(operation);
    void operation.catch(() => undefined);
  }
  const catalogError = controller.areasQuery.isError
    ? controller.areasQuery.error
    : controller.entidadesQuery.isError
      ? controller.entidadesQuery.error
      : undefined;
  return (
    <>
      <div className="mx-auto max-w-7xl space-y-6 pt-24">
        <PageHeader
          title="Programas Educativos"
          description="Administra los programas educativos de las entidades académicas."
          actions={
            <Button
              nativeButton={false}
              render={
                <Link href="/ProgramasEducativos/CrearProgramaEducativo" />
              }
            >
              <Plus />
              Crear Programa Educativo
            </Button>
          }
        />
        <ProgramasFiltersHeader
          consulta={controller.query}
          areas={controller.areas}
          areasLoading={controller.areasQuery.isPending}
          entidades={controller.entidades}
          entidadesLoading={controller.entidadesQuery.isPending}
          onChange={controller.updateFilters}
        />
        {controller.programasQuery.isError || catalogError ? (
          <p className="text-sm font-medium text-destructive" role="alert">
            {getErrorMessage(
              controller.programasQuery.isError
                ? controller.programasQuery.error
                : catalogError,
            )}
          </p>
        ) : null}
        <ProgramasDataGrid
          programas={controller.page?.items ?? []}
          isLoading={controller.programasQuery.isPending}
          isDeleting={controller.isDeleting}
          createHref="/ProgramasEducativos/CrearProgramaEducativo"
          getViewHref={getViewHref}
          getEditHref={getEditHref}
          getPlansHref={getPlansHref}
          onDelete={controller.requestDeletion}
        />
        <ProgramasPagination
          currentPage={controller.query.pagina}
          totalPages={controller.totalPages}
          isLoading={controller.programasQuery.isPending}
          onPageChange={controller.changePage}
        />
      </div>
      <DeleteProgramaEducativoDialog
        programaNombre={controller.pendingDeletion?.nombre}
        open={Boolean(controller.pendingDeletion)}
        onOpenChange={controller.setDeletionDialogOpen}
        onConfirm={confirmDeletion}
      />
    </>
  );
}

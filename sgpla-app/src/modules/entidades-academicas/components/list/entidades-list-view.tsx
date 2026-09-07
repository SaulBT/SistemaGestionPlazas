"use client";

import { useCallback, useState } from "react";
import Link from "next/link";
import { Plus } from "lucide-react";
import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";
import { getErrorMessage } from "@/shared/api/http-client";
import { useMinimumLoading } from "@/shared/hooks/use-minimum-loading";
import { MINIMUM_LOADING_DURATION_MS } from "@/shared/constants/loading";
import type {
  ConsultarEntidadesAcademicasQuery,
  EntidadAcademica,
} from "../../domain/entidad-academica";
import {
  useEliminarEntidadAcademica,
  useEntidadesAcademicas,
} from "../../presentation/entidades-academicas.queries";
import { EntidadesDataGrid } from "./entidades-data-grid";
import { EntidadesFiltersHeader } from "./entidades-filters-header";
import { DeleteEntidadAcademicaDialog } from "./ui/delete-entidad-academica-dialog";
import { showEntidadAcademicaDeletionToast } from "./ui/entidad-academica-notifications";
import { EntidadesPagination } from "./ui/entidades-pagination";

const consultaInicial: ConsultarEntidadesAcademicasQuery = {
  pagina: 1,
  cantidad: 10,
};

export function EntidadesListView() {
  const [consulta, setConsulta] =
    useState<ConsultarEntidadesAcademicasQuery>(consultaInicial);
  const entidadesQuery = useEntidadesAcademicas(consulta);
  const eliminarMutation = useEliminarEntidadAcademica();
  const isDeleting = useMinimumLoading(
    eliminarMutation.isPending,
    MINIMUM_LOADING_DURATION_MS,
    0,
  );
  const [entidadPendienteDeEliminar, setEntidadPendienteDeEliminar] =
    useState<EntidadAcademica | null>(null);
  const pagina = entidadesQuery.data;
  const totalPaginas = Math.max(
    1,
    Math.ceil((pagina?.total ?? 0) / consulta.cantidad),
  );

  const actualizarFiltros = useCallback(
    (cambios: Partial<ConsultarEntidadesAcademicasQuery>) => {
      setConsulta((actual) => ({ ...actual, ...cambios, pagina: 1 }));
    },
    [],
  );

  function cambiarPagina(nuevaPagina: number) {
    setConsulta((actual) => ({ ...actual, pagina: nuevaPagina }));
  }

  function solicitarEliminar(entidad: EntidadAcademica) {
    setEntidadPendienteDeEliminar(entidad);
  }

  async function confirmarEliminar() {
    if (!entidadPendienteDeEliminar) return;

    const entidad = entidadPendienteDeEliminar;
    setEntidadPendienteDeEliminar(null);
    const startedAt = Date.now();
    const waitForMinimumDuration = async () => {
      const remainingMs = Math.max(
        0,
        MINIMUM_LOADING_DURATION_MS - (Date.now() - startedAt),
      );

      if (remainingMs > 0) {
        await new Promise((resolve) => window.setTimeout(resolve, remainingMs));
      }
    };
    const deletionPromise = eliminarMutation
      .mutateAsync(entidad.idEntidadAcademica)
      .then(
        async (result) => {
          await waitForMinimumDuration();
          return result;
        },
        async (error) => {
          await waitForMinimumDuration();
          throw error;
        },
      );

    showEntidadAcademicaDeletionToast(deletionPromise);

    try {
      await deletionPromise;
    } catch {
      // El error se comunica mediante el Toast y el estado de la mutación.
    }
  }

  return (
    <Dashboard activeHref="/EntidadesAcademicas">
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
          consulta={consulta}
          onChange={actualizarFiltros}
        />

        {entidadesQuery.isError ? (
          <p className="text-sm font-medium text-destructive" role="alert">
            {getErrorMessage(entidadesQuery.error)}
          </p>
        ) : null}

        <EntidadesDataGrid
          entidades={pagina?.items ?? []}
          isLoading={entidadesQuery.isPending}
          isDeleting={isDeleting}
          onDelete={solicitarEliminar}
        />

        {eliminarMutation.isError ? (
          <p className="text-sm font-medium text-destructive" role="alert">
            {getErrorMessage(eliminarMutation.error)}
          </p>
        ) : null}

        <EntidadesPagination
          currentPage={consulta.pagina}
          totalPages={totalPaginas}
          isLoading={entidadesQuery.isPending}
          onPageChange={cambiarPagina}
        />
      </div>

      <DeleteEntidadAcademicaDialog
        entidadNombre={entidadPendienteDeEliminar?.nombre}
        open={Boolean(entidadPendienteDeEliminar)}
        onOpenChange={(open) => {
          if (!open) setEntidadPendienteDeEliminar(null);
        }}
        onConfirm={() => void confirmarEliminar()}
      />
    </Dashboard>
  );
}

"use client";

import { useCallback, useState } from "react";
import Link from "next/link";
import { Plus } from "lucide-react";
import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";
import { getErrorMessage } from "@/shared/api/http-client";
import type {
  ConsultarEntidadesAcademicasQuery,
  EntidadAcademica,
} from "../domain/entidad-academica";
import {
  useEliminarEntidadAcademica,
  useEntidadesAcademicas,
} from "../presentation/entidades-academicas.queries";
import { EntidadesDataGrid } from "./entidades-data-grid";
import { EntidadesFiltersHeader } from "./entidades-filters-header";

const consultaInicial: ConsultarEntidadesAcademicasQuery = {
  pagina: 1,
  cantidad: 10,
};

export function EntidadesListView() {
  const [consulta, setConsulta] =
    useState<ConsultarEntidadesAcademicasQuery>(consultaInicial);
  const entidadesQuery = useEntidadesAcademicas(consulta);
  const eliminarMutation = useEliminarEntidadAcademica();
  const pagina = entidadesQuery.data;
  const totalPaginas = Math.max(
    1,
    Math.ceil((pagina?.total ?? 0) / consulta.cantidad),
  );

  const actualizarFiltros = useCallback((
    cambios: Partial<ConsultarEntidadesAcademicasQuery>,
  ) => {
    setConsulta((actual) => ({ ...actual, ...cambios, pagina: 1 }));
  }, []);

  function cambiarPagina(nuevaPagina: number) {
    setConsulta((actual) => ({ ...actual, pagina: nuevaPagina }));
  }

  async function eliminarEntidad(entidad: EntidadAcademica) {
    const confirmar = window.confirm(
      `¿Eliminar la entidad académica “${entidad.nombre}”?`,
    );

    if (!confirmar) return;

    await eliminarMutation.mutateAsync(entidad.idEntidadAcademica);
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
              render={<Link href="/EntidadesAcademicas/CrearEntidadAcademica" />}
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
            isDeleting={eliminarMutation.isPending}
          onDelete={eliminarEntidad}
        />

        {eliminarMutation.isError ? (
          <p className="text-sm font-medium text-destructive" role="alert">
            {getErrorMessage(eliminarMutation.error)}
          </p>
        ) : null}

        <nav
          aria-label="Paginación de entidades académicas"
          className="flex items-center justify-end gap-3 text-sm text-muted-foreground"
        >
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={consulta.pagina <= 1 || entidadesQuery.isPending}
            onClick={() => cambiarPagina(consulta.pagina - 1)}
          >
            Anterior
          </Button>
          <span>
            Página {consulta.pagina} de {totalPaginas}
          </span>
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={consulta.pagina >= totalPaginas || entidadesQuery.isPending}
            onClick={() => cambiarPagina(consulta.pagina + 1)}
          >
            Siguiente
          </Button>
        </nav>
      </div>
    </Dashboard>
  );
}

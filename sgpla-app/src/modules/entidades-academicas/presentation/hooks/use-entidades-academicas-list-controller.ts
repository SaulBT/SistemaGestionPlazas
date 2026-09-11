"use client";

import { useCallback, useMemo, useState } from "react";
import type { ConsultarEntidadesAcademicasQuery } from "../../application/entidades-academicas.contracts";
import type { EntidadAcademica } from "../../domain/entidad-academica";
import {
  useAreasAcademicas,
  useEliminarEntidadAcademica,
  useEntidadesAcademicas,
} from "../entidades-academicas.queries";

const initialQuery: ConsultarEntidadesAcademicasQuery = {
  pagina: 1,
  cantidad: 10,
};

export function useEntidadesAcademicasListController() {
  const [query, setQuery] =
    useState<ConsultarEntidadesAcademicasQuery>(initialQuery);
  const [pendingDeletion, setPendingDeletion] =
    useState<EntidadAcademica | null>(null);
  const entidadesQuery = useEntidadesAcademicas(query);
  const areasQuery = useAreasAcademicas();
  const eliminarMutation = useEliminarEntidadAcademica();
  const page = entidadesQuery.data;
  const totalPages = Math.max(1, Math.ceil((page?.total ?? 0) / query.cantidad));
  const areas = useMemo(
    () =>
      (areasQuery.data?.items ?? []).map((area) => ({
        value: area.idAreaAcademica.toString(),
        label: area.nombre,
      })),
    [areasQuery.data],
  );

  const updateFilters = useCallback(
    (changes: Partial<ConsultarEntidadesAcademicasQuery>) => {
      setQuery((current) => ({ ...current, ...changes, pagina: 1 }));
    },
    [],
  );

  const changePage = useCallback((pageNumber: number) => {
    setQuery((current) => ({ ...current, pagina: pageNumber }));
  }, []);

  async function confirmDeletion() {
    if (!pendingDeletion) return;

    const idEntidadAcademica = pendingDeletion.idEntidadAcademica;
    const shouldReturnToPreviousPage =
      page?.items.length === 1 && query.pagina > 1;
    setPendingDeletion(null);
    await eliminarMutation.mutateAsync(idEntidadAcademica);

    if (shouldReturnToPreviousPage) {
      setQuery((current) => ({ ...current, pagina: current.pagina - 1 }));
    }
  }

  return {
    areas,
    areasQuery,
    changePage,
    confirmDeletion,
    entidadesQuery,
    isDeleting: eliminarMutation.isPending,
    page,
    pendingDeletion,
    query,
    requestDeletion: setPendingDeletion,
    setDeletionDialogOpen: (open: boolean) => {
      if (!open) setPendingDeletion(null);
    },
    totalPages,
    updateFilters,
  };
}

"use client";

import { useCallback, useMemo, useState } from "react";
import type { ConsultarProgramasEducativosQuery } from "../../application/programas-educativos.contracts";
import type { ProgramaEducativo } from "../../domain/programa-educativo";
import {
  useAreasAcademicas,
  useEliminarProgramaEducativo,
  useEntidadesAcademicas,
  useProgramasEducativos,
} from "../programas-educativos.queries";

const initialQuery: ConsultarProgramasEducativosQuery = {
  pagina: 1,
  cantidad: 10,
};

export function useProgramasEducativosListController() {
  const [query, setQuery] =
    useState<ConsultarProgramasEducativosQuery>(initialQuery);
  const [pendingDeletion, setPendingDeletion] =
    useState<ProgramaEducativo | null>(null);
  const programasQuery = useProgramasEducativos(query);
  const areasQuery = useAreasAcademicas();
  const entidadesQuery = useEntidadesAcademicas();
  const eliminarMutation = useEliminarProgramaEducativo();
  const page = programasQuery.data;
  const totalPages = Math.max(
    1,
    Math.ceil((page?.total ?? 0) / query.cantidad),
  );
  const areas = useMemo(
    () =>
      (areasQuery.data?.items ?? []).map((area) => ({
        value: area.idAreaAcademica.toString(),
        label: area.nombre,
      })),
    [areasQuery.data],
  );
  const entidades = useMemo(
    () =>
      (entidadesQuery.data?.items ?? []).map((entidad) => ({
        value: entidad.idEntidadAcademica.toString(),
        label: entidad.nombre,
      })),
    [entidadesQuery.data],
  );

  const updateFilters = useCallback(
    (changes: Partial<ConsultarProgramasEducativosQuery>) => {
      setQuery((current) => ({ ...current, ...changes, pagina: 1 }));
    },
    [],
  );

  const changePage = useCallback((pageNumber: number) => {
    setQuery((current) => ({ ...current, pagina: pageNumber }));
  }, []);

  async function confirmDeletion() {
    if (!pendingDeletion) return;

    const idProgramaEducativo = pendingDeletion.idProgramaEducativo;
    const shouldReturnToPreviousPage =
      page?.items.length === 1 && query.pagina > 1;
    setPendingDeletion(null);
    await eliminarMutation.mutateAsync(idProgramaEducativo);

    if (shouldReturnToPreviousPage) {
      setQuery((current) => ({ ...current, pagina: current.pagina - 1 }));
    }
  }

  return {
    areas,
    areasQuery,
    changePage,
    confirmDeletion,
    entidades,
    entidadesQuery,
    isDeleting: eliminarMutation.isPending,
    page,
    pendingDeletion,
    programasQuery,
    query,
    requestDeletion: setPendingDeletion,
    setDeletionDialogOpen: (open: boolean) => {
      if (!open) setPendingDeletion(null);
    },
    totalPages,
    updateFilters,
  };
}

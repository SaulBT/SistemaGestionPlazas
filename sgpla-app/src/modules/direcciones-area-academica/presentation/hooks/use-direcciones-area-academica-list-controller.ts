"use client";

import { useCallback, useState } from "react";
import type { ConsultarDireccionesAreaAcademicaQuery } from "../../application/direcciones-area-academica.contracts";
import type { DireccionAreaAcademica } from "../../domain/direccion-area-academica";
import {
  useDireccionesAreaAcademica,
  useEliminarDireccionAreaAcademica,
} from "../direcciones-area-academica.queries";

const initialQuery: ConsultarDireccionesAreaAcademicaQuery = {
  pagina: 1,
  cantidad: 10,
};

export function useDireccionesAreaAcademicaListController() {
  const [query, setQuery] =
    useState<ConsultarDireccionesAreaAcademicaQuery>(initialQuery);
  const [pendingDeletion, setPendingDeletion] =
    useState<DireccionAreaAcademica | null>(null);
  const direccionesQuery = useDireccionesAreaAcademica(query);
  const eliminarMutation = useEliminarDireccionAreaAcademica();
  const page = direccionesQuery.data;
  const totalPages = Math.max(1, Math.ceil((page?.total ?? 0) / query.cantidad));

  const updateFilters = useCallback(
    (changes: Partial<ConsultarDireccionesAreaAcademicaQuery>) => {
      setQuery((current) => ({ ...current, ...changes, pagina: 1 }));
    },
    [],
  );

  const changePage = useCallback((pageNumber: number) => {
    setQuery((current) => ({ ...current, pagina: pageNumber }));
  }, []);

  async function confirmDeletion() {
    if (!pendingDeletion) return;

    const idAreaAcademica = pendingDeletion.idAreaAcademica;
    const shouldReturnToPreviousPage =
      page?.items.length === 1 && query.pagina > 1;
    setPendingDeletion(null);
    await eliminarMutation.mutateAsync(idAreaAcademica);

    if (shouldReturnToPreviousPage) {
      setQuery((current) => ({ ...current, pagina: current.pagina - 1 }));
    }
  }

  return {
    changePage,
    confirmDeletion,
    direccionesQuery,
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
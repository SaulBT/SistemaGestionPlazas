"use client";

import { useState } from "react";
import type { Articulo } from "../../domain/articulo";
import { useArticulos, useEliminarArticulo } from "../articulos.queries";

export function useArticulosListController() {
  const [busqueda, setBusqueda] = useState("");
  const [pendiente, setPendiente] = useState<Articulo | null>(null);
  const articulosQuery = useArticulos(busqueda);
  const eliminarMutation = useEliminarArticulo();

  async function confirmarEliminacion() {
    if (!pendiente) return;
    const id = pendiente.idArticulo;
    setPendiente(null);
    await eliminarMutation.mutateAsync(id);
  }

  return {
    articulos: articulosQuery.data ?? [],
    articulosQuery,
    busqueda,
    confirmarEliminacion,
    isDeleting: eliminarMutation.isPending,
    pendiente,
    setBusqueda,
    setPendiente,
  };
}

"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useDebouncedValue } from "@/shared/hooks/use-debounced-value";
import { articulosPort } from "../composition";
import { guardarArticulo } from "../application/guardar-articulo";
import type { Articulo } from "../domain/articulo";
import type { GuardarArticuloInput } from "../application/articulos.contracts";

export const articulosKeys = {
  all: ["articulos"] as const,
  list: (busqueda?: string) =>
    [...articulosKeys.all, "lista", busqueda ?? ""] as const,
  detail: (id: number) => [...articulosKeys.all, "detalle", id] as const,
};

export function useArticulos(busqueda?: string) {
  const busquedaDebounced = useDebouncedValue(busqueda);
  return useQuery({
    queryKey: articulosKeys.list(busquedaDebounced?.trim()),
    queryFn: () =>
      articulosPort.consultar({ busqueda: busquedaDebounced?.trim() }),
    retry: false,
  });
}

export function useArticulo(idArticulo: number | undefined) {
  return useQuery({
    queryKey: articulosKeys.detail(idArticulo ?? 0),
    queryFn: () => articulosPort.consultarPorId(idArticulo!),
    enabled: Boolean(idArticulo && idArticulo > 0),
    retry: false,
  });
}

export function useGuardarArticulo() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: ({
      input,
      articuloExistente,
    }: {
      input: GuardarArticuloInput;
      articuloExistente?: Articulo;
    }) => guardarArticulo(articulosPort, input, articuloExistente),
    onSuccess: () => client.invalidateQueries({ queryKey: articulosKeys.all }),
  });
}

export function useEliminarArticulo() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: (idArticulo: number) => articulosPort.eliminar(idArticulo),
    onSuccess: () => client.invalidateQueries({ queryKey: articulosKeys.all }),
  });
}

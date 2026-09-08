"use client";

import { useMemo } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useDebouncedValue } from "@/shared/hooks/use-debounced-value";
import { guardarProgramaEducativo } from "../application/guardar-programa-educativo";
import type {
  ConsultarProgramasEducativosQuery,
  GuardarProgramaEducativoInput,
} from "../application/programas-educativos.contracts";
import type { ProgramaEducativo } from "../domain/programa-educativo";
import { programasEducativosPort } from "../composition";

export const programasEducativosKeys = {
  all: ["programas-educativos"] as const,
  lists: () => [...programasEducativosKeys.all, "lista"] as const,
  list: (query: ConsultarProgramasEducativosQuery) =>
    [...programasEducativosKeys.lists(), query] as const,
  details: () => [...programasEducativosKeys.all, "detalle"] as const,
  detail: (idProgramaEducativo: number) =>
    [...programasEducativosKeys.details(), idProgramaEducativo] as const,
  areas: () => ["areas-academicas", "catalogo"] as const,
  entidades: () => ["entidades-academicas", "catalogo"] as const,
};

export function useProgramasEducativos(
  query: ConsultarProgramasEducativosQuery,
) {
  const busquedaDebounced = useDebouncedValue(query.busqueda);
  const consultaDebounced = useMemo(() => {
    const busqueda = busquedaDebounced?.trim();

    return {
      ...query,
      busqueda: busqueda && busqueda.length >= 3 ? busqueda : undefined,
    };
  }, [busquedaDebounced, query]);

  return useQuery({
    queryKey: programasEducativosKeys.list(consultaDebounced),
    queryFn: () => programasEducativosPort.consultar(consultaDebounced),
    retry: false,
  });
}

export function useProgramaEducativo(idProgramaEducativo: number | undefined) {
  return useQuery({
    queryKey: programasEducativosKeys.detail(idProgramaEducativo ?? 0),
    queryFn: () => programasEducativosPort.consultarPorId(idProgramaEducativo!),
    enabled: Boolean(idProgramaEducativo && idProgramaEducativo > 0),
    retry: false,
  });
}

export function useAreasAcademicas() {
  return useQuery({
    queryKey: programasEducativosKeys.areas(),
    queryFn: () => programasEducativosPort.consultarAreasAcademicas(),
    retry: false,
  });
}

export function useEntidadesAcademicas() {
  return useQuery({
    queryKey: programasEducativosKeys.entidades(),
    queryFn: () => programasEducativosPort.consultarEntidadesAcademicas(),
    retry: false,
  });
}

function useInvalidarProgramasEducativos() {
  const queryClient = useQueryClient();

  return () =>
    queryClient.invalidateQueries({ queryKey: programasEducativosKeys.all });
}

export function useGuardarProgramaEducativo() {
  const invalidar = useInvalidarProgramasEducativos();

  return useMutation({
    mutationFn: ({
      input,
      programaExistente,
    }: {
      input: GuardarProgramaEducativoInput;
      programaExistente?: ProgramaEducativo;
    }) =>
      guardarProgramaEducativo(
        programasEducativosPort,
        input,
        programaExistente,
      ),
    onSuccess: invalidar,
  });
}

export function useEliminarProgramaEducativo() {
  const invalidar = useInvalidarProgramasEducativos();

  return useMutation({
    mutationFn: (idProgramaEducativo: number) =>
      programasEducativosPort.eliminar(idProgramaEducativo),
    onSuccess: invalidar,
  });
}

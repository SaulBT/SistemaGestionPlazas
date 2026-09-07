"use client";

import { useMemo } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useDebouncedValue } from "@/shared/hooks/use-debounced-value";
import { guardarEntidadAcademica } from "../application/guardar-entidad-academica";
import type { EntidadAcademica } from "../domain/entidad-academica";
import type {
  ConsultarEntidadesAcademicasQuery,
  GuardarEntidadAcademicaInput,
} from "../application/entidades-academicas.contracts";
import { entidadesAcademicasPort } from "../composition";

export const entidadesAcademicasKeys = {
  all: ["entidades-academicas"] as const,
  lists: () => [...entidadesAcademicasKeys.all, "lista"] as const,
  list: (query: ConsultarEntidadesAcademicasQuery) =>
    [...entidadesAcademicasKeys.lists(), query] as const,
  details: () => [...entidadesAcademicasKeys.all, "detalle"] as const,
  detail: (idEntidadAcademica: number) =>
    [...entidadesAcademicasKeys.details(), idEntidadAcademica] as const,
  areas: () => ["areas-academicas", "catalogo"] as const,
};

export function useEntidadesAcademicas(
  query: ConsultarEntidadesAcademicasQuery,
) {
  const busquedaDebounced = useDebouncedValue(query.busqueda);
  const consultaDebounced = useMemo(
    () => {
      const busqueda = busquedaDebounced?.trim();

      return {
        ...query,
        busqueda: busqueda && busqueda.length >= 3 ? busqueda : undefined,
      };
    },
    [busquedaDebounced, query],
  );

  return useQuery({
    queryKey: entidadesAcademicasKeys.list(consultaDebounced),
    queryFn: () =>
      entidadesAcademicasPort.consultar(consultaDebounced),
    retry: false,
  });
}

export function useEntidadAcademica(idEntidadAcademica: number | undefined) {
  return useQuery({
    queryKey: entidadesAcademicasKeys.detail(idEntidadAcademica ?? 0),
    queryFn: () =>
      entidadesAcademicasPort.consultarPorId(idEntidadAcademica!),
    enabled: Boolean(idEntidadAcademica && idEntidadAcademica > 0),
    retry: false,
  });
}

export function useAreasAcademicas() {
  return useQuery({
    queryKey: entidadesAcademicasKeys.areas(),
    queryFn: () => entidadesAcademicasPort.consultarAreasAcademicas(),
    retry: false,
  });
}

function useInvalidarEntidadesAcademicas() {
  const queryClient = useQueryClient();

  return () =>
    queryClient.invalidateQueries({ queryKey: entidadesAcademicasKeys.all });
}

export function useGuardarEntidadAcademica() {
  const invalidar = useInvalidarEntidadesAcademicas();

  return useMutation({
    mutationFn: ({
      input,
      entidadExistente,
    }: {
      input: GuardarEntidadAcademicaInput;
      entidadExistente?: EntidadAcademica;
    }) =>
      guardarEntidadAcademica(entidadesAcademicasPort, input, entidadExistente),
    onSuccess: invalidar,
  });
}

export function useEliminarEntidadAcademica() {
  const invalidar = useInvalidarEntidadesAcademicas();

  return useMutation({
    mutationFn: (idEntidadAcademica: number) =>
      entidadesAcademicasPort.eliminar(idEntidadAcademica),
    onSuccess: invalidar,
  });
}

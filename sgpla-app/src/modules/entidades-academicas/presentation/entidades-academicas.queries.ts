"use client";

import { useMemo } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useDebouncedValue } from "@/shared/hooks/use-debounced-value";
import type {
  ConsultarEntidadesAcademicasQuery,
  GuardarEntidadAcademicaInput,
} from "../domain/entidad-academica";
import { httpEntidadesAcademicasAdapter } from "../infra/http-entidades-academicas.adapter";

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
      httpEntidadesAcademicasAdapter.consultar(consultaDebounced),
    retry: false,
  });
}

export function useEntidadAcademica(idEntidadAcademica: number | undefined) {
  return useQuery({
    queryKey: entidadesAcademicasKeys.detail(idEntidadAcademica ?? 0),
    queryFn: () =>
      httpEntidadesAcademicasAdapter.consultarPorId(idEntidadAcademica!),
    enabled: Boolean(idEntidadAcademica && idEntidadAcademica > 0),
    retry: false,
  });
}

export function useAreasAcademicas() {
  return useQuery({
    queryKey: entidadesAcademicasKeys.areas(),
    queryFn: () => httpEntidadesAcademicasAdapter.consultarAreasAcademicas(),
    retry: false,
  });
}

function useInvalidarEntidadesAcademicas() {
  const queryClient = useQueryClient();

  return () =>
    queryClient.invalidateQueries({ queryKey: entidadesAcademicasKeys.all });
}

export function useCrearEntidadAcademica() {
  const invalidar = useInvalidarEntidadesAcademicas();

  return useMutation({
    mutationFn: (input: GuardarEntidadAcademicaInput) =>
      httpEntidadesAcademicasAdapter.crear(input),
    onSuccess: invalidar,
  });
}

export function useActualizarEntidadAcademica() {
  const invalidar = useInvalidarEntidadesAcademicas();

  return useMutation({
    mutationFn: ({
      idEntidadAcademica,
      input,
    }: {
      idEntidadAcademica: number;
      input: GuardarEntidadAcademicaInput;
    }) => httpEntidadesAcademicasAdapter.actualizar(idEntidadAcademica, input),
    onSuccess: invalidar,
  });
}

export function useEliminarEntidadAcademica() {
  const invalidar = useInvalidarEntidadesAcademicas();

  return useMutation({
    mutationFn: (idEntidadAcademica: number) =>
      httpEntidadesAcademicasAdapter.eliminar(idEntidadAcademica),
    onSuccess: invalidar,
  });
}

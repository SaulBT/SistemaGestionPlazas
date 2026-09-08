"use client";

import { useMemo } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useDebouncedValue } from "@/shared/hooks/use-debounced-value";
import { guardarDireccionAreaAcademica } from "../application/guardar-direccion-area-academica";
import type {
  ConsultarDireccionesAreaAcademicaQuery,
  GuardarDireccionAreaAcademicaInput,
} from "../application/direcciones-area-academica.contracts";
import type { DireccionAreaAcademica } from "../domain/direccion-area-academica";
import { direccionesAreaAcademicaPort } from "../composition";

export const direccionesAreaAcademicaKeys = {
  all: ["direcciones-area-academica"] as const,
  lists: () => [...direccionesAreaAcademicaKeys.all, "lista"] as const,
  list: (query: ConsultarDireccionesAreaAcademicaQuery) =>
    [...direccionesAreaAcademicaKeys.lists(), query] as const,
  details: () => [...direccionesAreaAcademicaKeys.all, "detalle"] as const,
  detail: (idAreaAcademica: number) =>
    [...direccionesAreaAcademicaKeys.details(), idAreaAcademica] as const,
};

export function useDireccionesAreaAcademica(
  query: ConsultarDireccionesAreaAcademicaQuery,
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
    queryKey: direccionesAreaAcademicaKeys.list(consultaDebounced),
    queryFn: () => direccionesAreaAcademicaPort.consultar(consultaDebounced),
    retry: false,
  });
}

export function useDireccionAreaAcademica(idAreaAcademica: number | undefined) {
  return useQuery({
    queryKey: direccionesAreaAcademicaKeys.detail(idAreaAcademica ?? 0),
    queryFn: () => direccionesAreaAcademicaPort.consultarPorId(idAreaAcademica!),
    enabled: Boolean(idAreaAcademica && idAreaAcademica > 0),
    retry: false,
  });
}

function useInvalidarDireccionesAreaAcademica() {
  const queryClient = useQueryClient();

  return () =>
    queryClient.invalidateQueries({ queryKey: direccionesAreaAcademicaKeys.all });
}

export function useGuardarDireccionAreaAcademica() {
  const invalidar = useInvalidarDireccionesAreaAcademica();

  return useMutation({
    mutationFn: ({
      input,
      direccionExistente,
    }: {
      input: GuardarDireccionAreaAcademicaInput;
      direccionExistente?: DireccionAreaAcademica;
    }) =>
      guardarDireccionAreaAcademica(
        direccionesAreaAcademicaPort,
        input,
        direccionExistente,
      ),
    onSuccess: invalidar,
  });
}

export function useEliminarDireccionAreaAcademica() {
  const invalidar = useInvalidarDireccionesAreaAcademica();

  return useMutation({
    mutationFn: (idAreaAcademica: number) =>
      direccionesAreaAcademicaPort.eliminar(idAreaAcademica),
    onSuccess: invalidar,
  });
}
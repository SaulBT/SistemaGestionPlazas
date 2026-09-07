"use client";

import { getErrorMessage } from "@/shared/api/http-client";
import { useMinimumLoading } from "@/shared/hooks/use-minimum-loading";
import { MINIMUM_LOADING_DURATION_MS } from "@/shared/constants/loading";
import { useEntidadAcademica } from "../presentation/entidades-academicas.queries";
import { EntidadAcademicaFormPage } from "./entidad-academica-form-page";

type EntidadAcademicaFormPageLoaderProps = {
  idEntidadAcademica: number | undefined;
  readOnly?: boolean;
  title: string;
  description: string;
};

export function EntidadAcademicaFormPageLoader({
  idEntidadAcademica,
  readOnly = false,
  title,
  description,
}: EntidadAcademicaFormPageLoaderProps) {
  const entidadQuery = useEntidadAcademica(idEntidadAcademica);
  const shouldShowLoading = useMinimumLoading(
    entidadQuery.isPending,
    MINIMUM_LOADING_DURATION_MS,
    150,
  );

  if (!idEntidadAcademica || idEntidadAcademica <= 0) {
    return (
      <p className="pt-6 text-sm font-medium text-destructive" role="alert">
        El identificador de la entidad académica no es válido.
      </p>
    );
  }

  if (shouldShowLoading) {
    return (
      <p className="pt-6 text-sm text-muted-foreground">
        Cargando entidad académica…
      </p>
    );
  }

  if (entidadQuery.isPending) return null;

  if (entidadQuery.isError || !entidadQuery.data) {
    return (
      <p className="pt-6 text-sm font-medium text-destructive" role="alert">
        {entidadQuery.isError
          ? getErrorMessage(entidadQuery.error)
          : "No se encontró la entidad académica solicitada."}
      </p>
    );
  }

  return (
    <EntidadAcademicaFormPage
      key={entidadQuery.data.idEntidadAcademica}
      entidad={entidadQuery.data}
      readOnly={readOnly}
      title={title}
      description={description}
    />
  );
}

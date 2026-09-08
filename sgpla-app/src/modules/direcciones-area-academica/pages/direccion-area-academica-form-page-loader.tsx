"use client";

import { getErrorMessage } from "@/shared/api/http-client";
import { MINIMUM_LOADING_DURATION_MS } from "@/shared/constants/loading";
import { useMinimumLoading } from "@/shared/hooks/use-minimum-loading";
import { useDireccionAreaAcademica } from "../presentation/direcciones-area-academica.queries";
import { DireccionAreaAcademicaFormPage } from "./direccion-area-academica-form-page";

type Props = {
  idAreaAcademica: number | undefined;
  readOnly?: boolean;
  title: string;
  description: string;
};

export function DireccionAreaAcademicaFormPageLoader({
  idAreaAcademica,
  readOnly = false,
  title,
  description,
}: Props) {
  const direccionQuery = useDireccionAreaAcademica(idAreaAcademica);
  const shouldShowLoading = useMinimumLoading(
    direccionQuery.isPending,
    MINIMUM_LOADING_DURATION_MS,
    150,
  );

  if (!idAreaAcademica || idAreaAcademica <= 0) {
    return (
      <p className="pt-6 text-sm font-medium text-destructive" role="alert">
        El identificador de la dirección de área académica no es válido.
      </p>
    );
  }

  if (shouldShowLoading) {
    return (
      <p className="pt-6 text-sm text-muted-foreground">
        Cargando dirección de área académica…
      </p>
    );
  }

  if (direccionQuery.isPending) return null;

  if (direccionQuery.isError || !direccionQuery.data) {
    return (
      <p className="pt-6 text-sm font-medium text-destructive" role="alert">
        {direccionQuery.isError
          ? getErrorMessage(direccionQuery.error)
          : "No se encontró la dirección de área académica solicitada."}
      </p>
    );
  }

  return (
    <DireccionAreaAcademicaFormPage
      key={direccionQuery.data.idAreaAcademica}
      direccion={direccionQuery.data}
      readOnly={readOnly}
      title={title}
      description={description}
    />
  );
}
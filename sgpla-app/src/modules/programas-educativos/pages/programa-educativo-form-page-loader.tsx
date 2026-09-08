"use client";
import { getErrorMessage } from "@/shared/api/http-client";
import { MINIMUM_LOADING_DURATION_MS } from "@/shared/constants/loading";
import { useMinimumLoading } from "@/shared/hooks/use-minimum-loading";
import { useProgramaEducativo } from "../presentation/programas-educativos.queries";
import { ProgramaEducativoFormPage } from "./programa-educativo-form-page";
type Props = {
  idProgramaEducativo: number | undefined;
  readOnly?: boolean;
  title: string;
  description: string;
};
export function ProgramaEducativoFormPageLoader({
  idProgramaEducativo,
  readOnly = false,
  title,
  description,
}: Props) {
  const programaQuery = useProgramaEducativo(idProgramaEducativo);
  const shouldShowLoading = useMinimumLoading(
    programaQuery.isPending,
    MINIMUM_LOADING_DURATION_MS,
    150,
  );
  if (!idProgramaEducativo || idProgramaEducativo <= 0)
    return (
      <p className="pt-6 text-sm font-medium text-destructive" role="alert">
        El identificador del programa educativo no es válido.
      </p>
    );
  if (shouldShowLoading)
    return (
      <p className="pt-6 text-sm text-muted-foreground">
        Cargando programa educativo…
      </p>
    );
  if (programaQuery.isPending) return null;
  if (programaQuery.isError || !programaQuery.data)
    return (
      <p className="pt-6 text-sm font-medium text-destructive" role="alert">
        {programaQuery.isError
          ? getErrorMessage(programaQuery.error)
          : "No se encontró el programa educativo solicitado."}
      </p>
    );
  return (
    <ProgramaEducativoFormPage
      key={programaQuery.data.idProgramaEducativo}
      programa={programaQuery.data}
      readOnly={readOnly}
      title={title}
      description={description}
    />
  );
}

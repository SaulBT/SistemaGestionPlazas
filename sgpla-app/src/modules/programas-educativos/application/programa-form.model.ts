import type {
  PlanEstudio,
  ProgramaEducativoDetalle,
} from "../domain/programa-educativo";
import type { GuardarProgramaEducativoInput } from "./programas-educativos.contracts";

export type ProgramaFormValues = {
  clave: string;
  nombre: string;
  campus: string;
  idEntidadAcademica: string;
  idAreaAcademica: string;
  region: string;
  planesEstudio: PlanEstudioFormValues[];
};

export type PlanEstudioFormValues = {
  idPlanEstudios?: number;
  nombre: string;
  modalidad: string;
  archivo?: File;
  nombreArchivo: string;
  cantidadExperienciasEducativas?: number;
};

function separarClaveYNombre(nombreCompleto: string) {
  const separador = nombreCompleto.indexOf("-");

  return separador < 0
    ? { clave: "", nombre: nombreCompleto }
    : {
        clave: nombreCompleto.slice(0, separador).trim(),
        nombre: nombreCompleto.slice(separador + 1).trim(),
      };
}

export function toProgramaFormValues(
  programa?: ProgramaEducativoDetalle,
): ProgramaFormValues {
  const { clave, nombre } = separarClaveYNombre(programa?.nombre ?? "");

  return {
    clave,
    nombre,
    campus: programa?.campus ?? "",
    idEntidadAcademica: programa?.idEntidadAcademica.toString() ?? "",
    idAreaAcademica: programa?.idAreaAcademica.toString() ?? "",
    region: programa?.region ?? "",
    planesEstudio: (programa?.planesEstudio ?? []).map(toPlanEstudioFormValues),
  };
}

function toPlanEstudioFormValues(
  planEstudio: PlanEstudio,
): PlanEstudioFormValues {
  return {
    idPlanEstudios: planEstudio.idPlanEstudios,
    nombre: planEstudio.nombre,
    modalidad: planEstudio.modalidad ?? "",
    nombreArchivo: planEstudio.nombreArchivo ?? "",
    cantidadExperienciasEducativas: planEstudio.cantidadExperienciasEducativas,
  };
}

export function toGuardarProgramaEducativoInput(
  values: ProgramaFormValues,
): GuardarProgramaEducativoInput {
  return {
    nombre: `${values.clave.trim()}-${values.nombre.trim()}`,
    campus: values.campus.trim(),
    idEntidadAcademica: Number(values.idEntidadAcademica),
    planesEstudio: values.planesEstudio.map((plan) => ({
      idPlanEstudios: plan.idPlanEstudios,
      nombre: plan.nombre.trim(),
      modalidad: plan.modalidad.trim(),
      archivo: plan.archivo,
    })),
  };
}

function hasPlanEstudioChanges(
  values: PlanEstudioFormValues,
  originalValues: PlanEstudioFormValues,
) {
  return (
    values.idPlanEstudios !== originalValues.idPlanEstudios ||
    values.nombre.trim() !== originalValues.nombre.trim() ||
    values.modalidad.trim() !== originalValues.modalidad.trim() ||
    values.archivo !== undefined
  );
}

export function hasProgramaFormChanges(
  values: ProgramaFormValues,
  originalValues: ProgramaFormValues,
) {
  const fields: Array<Exclude<keyof ProgramaFormValues, "planesEstudio">> = [
    "clave",
    "nombre",
    "campus",
    "idEntidadAcademica",
    "idAreaAcademica",
    "region",
  ];

  return (
    fields.some(
      (field) => values[field].trim() !== originalValues[field].trim(),
    ) ||
    values.planesEstudio.length !== originalValues.planesEstudio.length ||
    values.planesEstudio.some((plan, index) => {
      const originalPlan = originalValues.planesEstudio[index];
      return !originalPlan || hasPlanEstudioChanges(plan, originalPlan);
    })
  );
}

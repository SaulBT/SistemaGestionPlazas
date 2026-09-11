import type { EntidadesAcademicasPort } from "./application/entidades-academicas.port";
import { httpEntidadesAcademicasAdapter } from "./infra/http-entidades-academicas.adapter";

export const entidadesAcademicasPort: EntidadesAcademicasPort =
  httpEntidadesAcademicasAdapter;

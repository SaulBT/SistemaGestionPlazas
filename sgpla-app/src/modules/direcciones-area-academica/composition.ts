import type { DireccionesAreaAcademicaPort } from "./application/direcciones-area-academica.port";
import { httpDireccionesAreaAcademicaAdapter } from "./infra/http-direcciones-area-academica.adapter";

export const direccionesAreaAcademicaPort: DireccionesAreaAcademicaPort =
  httpDireccionesAreaAcademicaAdapter;
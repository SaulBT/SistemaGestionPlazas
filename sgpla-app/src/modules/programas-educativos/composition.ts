import type { ProgramasEducativosPort } from "./application/programas-educativos.port";
import { httpProgramasEducativosAdapter } from "./infra/http-programas-educativos.adapter";

export const programasEducativosPort: ProgramasEducativosPort =
  httpProgramasEducativosAdapter;

import type { DireccionAreaAcademica } from "../domain/direccion-area-academica";
import type { DireccionesAreaAcademicaPort } from "./direcciones-area-academica.port";
import type { GuardarDireccionAreaAcademicaInput } from "./direcciones-area-academica.contracts";

export function guardarDireccionAreaAcademica(
  port: DireccionesAreaAcademicaPort,
  input: GuardarDireccionAreaAcademicaInput,
  direccionExistente?: DireccionAreaAcademica,
) {
  return direccionExistente
    ? port.actualizar(direccionExistente.idAreaAcademica, input)
    : port.crear(input);
}
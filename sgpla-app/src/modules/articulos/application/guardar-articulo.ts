import type {
  ArticulosPort,
  GuardarArticuloInput,
} from "./articulos.contracts";
import type { Articulo } from "../domain/articulo";

export function guardarArticulo(
  port: ArticulosPort,
  input: GuardarArticuloInput,
  articuloExistente?: Articulo,
) {
  return articuloExistente
    ? port.actualizar(articuloExistente.idArticulo, input)
    : port.crear(input);
}

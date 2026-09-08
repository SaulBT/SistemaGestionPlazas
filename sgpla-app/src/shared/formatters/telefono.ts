export function formatoTelefono(
  telefono: string | null | undefined,
): string {
  const valor = telefono?.trim() ?? "";
  const digitos = valor.replace(/\D/g, "");

  if (digitos.length !== 10) {
    return valor;
  }

  return `${digitos.slice(0, 3)} ${digitos.slice(3, 6)} ${digitos.slice(6)}`;
}

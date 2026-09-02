export type EntidadAcademica = {
  id: string;
  clave: string;
  nombre: string;
  calleNumero: string;
  colonia: string;
  cp: string;
  municipio: string;
  telefono: string;
  extension: string;
  areaAcademica: string;
  region: string;
  domicilio: string;
};

export const entidadesAcademicas: EntidadAcademica[] = [
  { id: "1", clave: "FI", nombre: "Facultad de Ingeniería", calleNumero: "Av. Ruiz Cortines 455", colonia: "Unidad del Bosque", cp: "91010", municipio: "Xalapa", telefono: "2288421700", extension: "12101", areaAcademica: "Área Académica Técnica", region: "1-Xalapa", domicilio: "Av. Ruiz Cortines 455, Unidad del Bosque, Xalapa" },
  { id: "2", clave: "FCQ", nombre: "Facultad de Ciencias Químicas", calleNumero: "Prol. Oriente 6 100", colonia: "Centro", cp: "94300", municipio: "Orizaba", telefono: "2727250100", extension: "12202", areaAcademica: "Área Académica de Ciencias de la Salud", region: "3-Orizaba-Córdoba", domicilio: "Prol. Oriente 6 100, Centro, Orizaba" },
  { id: "3", clave: "FP", nombre: "Facultad de Pedagogía", calleNumero: "Paseo 112", colonia: "Formando Hogar", cp: "91910", municipio: "Veracruz", telefono: "2299322000", extension: "12303", areaAcademica: "Área Académica de Humanidades", region: "2-Veracruz", domicilio: "Paseo 112, Formando Hogar, Veracruz" },
];

export const regiones = ["1-Xalapa", "2-Veracruz", "3-Orizaba-Córdoba", "4-Poza Rica-Túxpan", "5-Coatzacoalcos-Minatitlán"];
export const areasAcademicas = ["Área Académica Técnica", "Área Académica de Humanidades", "Área Académica de Ciencias de la Salud"];

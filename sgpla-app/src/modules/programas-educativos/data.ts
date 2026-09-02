export type ProgramaEducativo = {
  id: string;
  clave: string;
  nombre: string;
  campus: string;
  region: string;
  areaAcademica: string;
  entidadAcademica: string;
};

export const programasEducativos: ProgramaEducativo[] = [
  { id: "1", clave: "10001", nombre: "Ingeniería en Sistemas Computacionales", campus: "Xalapa", region: "1-Xalapa", areaAcademica: "Área Académica Técnica", entidadAcademica: "Facultad de Ingeniería" },
  { id: "2", clave: "10002", nombre: "Ingeniería Civil", campus: "Xalapa", region: "1-Xalapa", areaAcademica: "Área Académica Técnica", entidadAcademica: "Facultad de Ingeniería" },
  { id: "3", clave: "20001", nombre: "Licenciatura en Pedagogía", campus: "Veracruz", region: "2-Veracruz", areaAcademica: "Área Académica de Humanidades", entidadAcademica: "Facultad de Pedagogía" },
  { id: "4", clave: "30001", nombre: "Química Industrial", campus: "Orizaba", region: "3-Orizaba-Córdoba", areaAcademica: "Área Académica de Ciencias de la Salud", entidadAcademica: "Facultad de Ciencias Químicas" },
];

export const regiones = ["1-Xalapa", "2-Veracruz", "3-Orizaba-Córdoba", "4-Poza Rica-Túxpan", "5-Coatzacoalcos-Minatitlán"];
export const areasAcademicas = ["Área Académica Técnica", "Área Académica de Humanidades", "Área Académica de Ciencias de la Salud"];
export const entidadesAcademicas = ["Facultad de Ingeniería", "Facultad de Pedagogía", "Facultad de Ciencias Químicas"];

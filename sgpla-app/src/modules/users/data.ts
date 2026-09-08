export type UserListItem = {
  id: number;
  name: string;
  email: string;
  position: string;
  role: string;
  areaOrEntity: string;
  region: string;
};

export const userList: UserListItem[] = [
  {
    id: 1,
    name: "María Fernanda López",
    email: "maria.lopez@uv.mx",
    position: "Coordinadora académica",
    role: "Coordinador de Entidad Académica",
    areaOrEntity: "Facultad de Ingeniería",
    region: "1-Xalapa",
  },
  {
    id: 2,
    name: "Carlos Alberto Méndez",
    email: "carlos.mendez@uv.mx",
    position: "Responsable de área",
    role: "Coordinador de Área Académica",
    areaOrEntity: "Área Académica Técnica",
    region: "1-Xalapa",
  },
  {
    id: 3,
    name: "Ana Sofía Ramírez",
    email: "ana.ramirez@uv.mx",
    position: "Coordinadora académica",
    role: "Coordinador de Entidad Académica",
    areaOrEntity: "Facultad de Ciencias Químicas",
    region: "2-Veracruz",
  },
];

export const regions = [
  "1-Xalapa",
  "2-Veracruz",
  "3-Orizaba-Córdoba",
  "4-Poza Rica-Túxpan",
  "5-Coatzacoalcos-Minatitlán",
];

export const roles = [
  "Coordinador de Entidad Académica",
  "Coordinador de Área Académica",
];

export const academicAreas = [
  "Área Académica Técnica",
  "Área Académica de Humanidades",
  "Área Académica de Ciencias de la Salud",
];

export const academicEntities = [
  "Facultad de Ingeniería",
  "Facultad de Ciencias Químicas",
  "Facultad de Pedagogía",
];

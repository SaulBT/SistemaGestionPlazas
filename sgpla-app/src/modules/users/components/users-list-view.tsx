"use client";

import Link from "next/link";
import {
  Eye,
  Pencil,
  Plus,
  Search,
  Trash2,
  UserRound,
  Mail,
  BriefcaseBusiness,
  ShieldCheck,
  Building2,
  MapPin,
  MoreHorizontal,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/components/page-header";
import { Field, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { academicAreas, academicEntities, regions, userList } from "../data";
import { UserSelectField } from "./user-select-field";
import { DataGrid, type DataGridColumn } from "@/components/data-grid";

export function UsersListView() {
  const columns: DataGridColumn<
    | "name"
    | "email"
    | "position"
    | "role"
    | "areaOrEntity"
    | "region"
    | "actions"
  >[] = [
    { id: "name", label: "Nombre", icon: UserRound, defaultWidth: 190 },
    { id: "email", label: "Correo", icon: Mail, defaultWidth: 220 },
    {
      id: "position",
      label: "Cargo",
      icon: BriefcaseBusiness,
      defaultWidth: 180,
    },
    { id: "role", label: "Rol", icon: ShieldCheck, defaultWidth: 240 },
    {
      id: "areaOrEntity",
      label: "Entidad/Área",
      icon: Building2,
      defaultWidth: 210,
    },
    { id: "region", label: "Región", icon: MapPin, defaultWidth: 180 },
    {
      id: "actions",
      label: "Acciones",
      icon: MoreHorizontal,
      defaultWidth: 128,
    },
  ];
  const rows = userList.map((user) => ({ ...user, id: String(user.id) }));

  const renderCell = (
    row: (typeof rows)[number],
    column: DataGridColumn<(typeof columns)[number]["id"]>,
  ) => {
    if (column.id === "actions") {
      return (
        <div className="flex justify-end gap-1">
          <Button
            nativeButton={false}
            render={
              <Link
                href={`/Usuarios/VerUsuario?id=${row.id}&rol=${encodeURIComponent(row.role)}`}
              />
            }
            variant="ghost"
            size="icon-sm"
            aria-label={`Ver a ${row.name}`}
          >
            <Eye />
          </Button>
          <Button
            nativeButton={false}
            render={
              <Link
                href={`/Usuarios/EditarUsuario?id=${row.id}&rol=${encodeURIComponent(row.role)}`}
              />
            }
            variant="ghost"
            size="icon-sm"
            aria-label={`Editar a ${row.name}`}
          >
            <Pencil />
          </Button>
          <Button
            type="button"
            variant="ghost"
            size="icon-sm"
            aria-label={`Eliminar a ${row.name}`}
          >
            <Trash2 />
          </Button>
        </div>
      );
    }
    return row[column.id as keyof typeof row];
  };

  return (
    <div className="mx-auto max-w-7xl space-y-6 pt-24">
      <PageHeader
        title="Usuarios"
        description="Administra los usuarios y sus permisos dentro del sistema."
        actions={
          <Button
            nativeButton={false}
            render={<Link href="/Usuarios/CrearUsuario" />}
          >
            <Plus />
            Crear Usuario
          </Button>
        }
      />

      <form
        action="/Usuarios"
        method="get"
        className="grid items-end gap-3 sm:grid-cols-2 lg:grid-cols-[minmax(13rem,1.3fr)_repeat(3,minmax(10rem,1fr))_auto]"
      >
        <Field>
          <FieldLabel htmlFor="busqueda">Buscar usuario</FieldLabel>
          <div className="relative">
            <Search className="pointer-events-none absolute left-3 top-2 size-3.5 text-muted-foreground" />
            <Input
              id="busqueda"
              name="busqueda"
              placeholder="Buscar usuario..."
            />
          </div>
        </Field>
        <UserSelectField
          id="region"
          label="Región"
          name="region"
          options={regions}
        />
        <UserSelectField
          id="idAreaAcademica"
          label="Área Académica"
          name="idAreaAcademica"
          options={academicAreas}
        />
        <UserSelectField
          id="idEntidadAcademica"
          label="Entidad Académica"
          name="idEntidadAcademica"
          options={academicEntities}
        />
        <Button type="submit" variant="secondary" size="sm">
          <Search />
          Aplicar
        </Button>
      </form>

      <section>
        <DataGrid
          rows={rows}
          columns={columns}
          getRowLabel={(row) => row.name}
          renderCell={renderCell}
          isEditableColumn={() => false}
          getCellEditValue={(row, columnId) =>
            columnId === "actions"
              ? ""
              : String(row[columnId as keyof typeof row])
          }
          applyCellEdit={(row) => row}
          getDrawerCellValue={(row, columnId) =>
            columnId === "actions" ? null : row[columnId as keyof typeof row]
          }
          canOpenDrawer={() => false}
          enableRowSelection={false}
          tableContainerClassName="rounded-lg border border-border"
        />
      </section>

      <nav
        aria-label="Paginación de usuarios"
        className="flex items-center justify-end gap-2 text-sm"
      >
        <span className="mr-2 text-muted-foreground">Página 1 de 1</span>
        <Button
          nativeButton={false}
          render={<Link href="/Usuarios?pagina=1&cantidad=10" />}
          size="sm"
          variant="secondary"
        >
          Anterior
        </Button>
        <Button
          nativeButton={false}
          render={<Link href="/Usuarios?pagina=1&cantidad=10" />}
          size="sm"
          variant="secondary"
        >
          Siguiente
        </Button>
      </nav>
    </div>
  );
}

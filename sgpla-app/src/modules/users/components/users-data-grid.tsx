"use client";

import Link from "next/link";
import {
  BriefcaseBusiness,
  Building2,
  Eye,
  Mail,
  MapPin,
  MoreHorizontal,
  Pencil,
  ShieldCheck,
  Trash2,
  UserRound,
} from "lucide-react";

import { DataGrid, type DataGridColumn } from "@/components/data-grid";
import { Button } from "@/components/ui/button";
import { userList } from "../data";

type UserColumnId =
  | "name"
  | "email"
  | "position"
  | "role"
  | "areaOrEntity"
  | "region"
  | "actions";

type UserGridRow = Omit<(typeof userList)[number], "id"> & { id: string };

const columns: DataGridColumn<UserColumnId>[] = [
  { id: "name", label: "Nombre", icon: UserRound, defaultWidth: 190 },
  { id: "email", label: "Correo", icon: Mail, defaultWidth: 220 },
  { id: "position", label: "Cargo", icon: BriefcaseBusiness, defaultWidth: 180 },
  { id: "role", label: "Rol", icon: ShieldCheck, defaultWidth: 240 },
  { id: "areaOrEntity", label: "Entidad/Área", icon: Building2, defaultWidth: 210 },
  { id: "region", label: "Región", icon: MapPin, defaultWidth: 180 },
  {
    id: "actions",
    label: "Acciones",
    icon: MoreHorizontal,
    defaultWidth: 128,
    isActions: true,
  },
];

export function UsersDataGrid() {
  const rows: UserGridRow[] = userList.map((user) => ({
    ...user,
    id: String(user.id),
  }));

  function renderCell(row: UserGridRow, column: DataGridColumn<UserColumnId>) {
    if (column.id === "actions") {
      return (
        <div className="flex justify-end gap-1">
          <Button
            nativeButton={false}
            render={<Link href={`/Usuarios/VerUsuario?id=${row.id}&rol=${encodeURIComponent(row.role)}`} />}
            variant="ghost"
            size="icon-sm"
            aria-label={`Ver a ${row.name}`}
          >
            <Eye />
          </Button>
          <Button
            nativeButton={false}
            render={<Link href={`/Usuarios/EditarUsuario?id=${row.id}&rol=${encodeURIComponent(row.role)}`} />}
            variant="ghost"
            size="icon-sm"
            aria-label={`Editar a ${row.name}`}
          >
            <Pencil />
          </Button>
          <Button type="button" variant="ghost" size="icon-sm" aria-label={`Eliminar a ${row.name}`}>
            <Trash2 />
          </Button>
        </div>
      );
    }

    return row[column.id as keyof UserGridRow];
  }

  return (
    <DataGrid
        rows={rows}
        columns={columns}
        getRowLabel={(row) => row.name}
        renderCell={renderCell}
        isEditableColumn={() => false}
        getCellEditValue={(row, columnId) =>
          columnId === "actions" ? "" : String(row[columnId as keyof UserGridRow])
        }
        applyCellEdit={(row) => row}
        getDrawerCellValue={(row, columnId) =>
          columnId === "actions" ? null : row[columnId as keyof UserGridRow]
        }
        canOpenDrawer={() => false}
        enableRowSelection={false}
      />
  );
}

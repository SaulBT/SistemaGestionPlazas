"use client";

import Link from "next/link";
import { Eye, MoreHorizontal, Pencil, Phone, Trash2 } from "lucide-react";

import { DataGrid, type DataGridColumn } from "@/components/data-grid";
import { Button } from "@/components/ui/button";

import { direccionesAreaAcademica, type DireccionAreaAcademica } from "../data";

type DireccionColumnId = "nombre" | "telefono" | "acciones";

const columns: DataGridColumn<DireccionColumnId>[] = [
  { id: "nombre", label: "Nombre de la Dirección", icon: MoreHorizontal, defaultWidth: 430 },
  { id: "telefono", label: "Teléfono", icon: Phone, defaultWidth: 190 },
  {
    id: "acciones",
    label: "Acciones",
    icon: MoreHorizontal,
    defaultWidth: 160,
    isActions: true,
  },
];

export function DireccionesDataGrid() {
  function renderCell(
    row: DireccionAreaAcademica,
    column: DataGridColumn<DireccionColumnId>,
  ) {
    if (column.id === "acciones") {
      return (
        <div className="flex justify-end gap-1">
          <Button
            nativeButton={false}
            render={<Link href={`/DireccionesAreaAcademica/VerDireccionAreaAcademica?id=${row.id}`} />}
            variant="ghost"
            size="icon-sm"
            aria-label={`Ver ${row.nombre}`}
          >
            <Eye />
          </Button>
          <Button
            nativeButton={false}
            render={<Link href={`/DireccionesAreaAcademica/EditarDireccionAreaAcademica?id=${row.id}`} />}
            variant="ghost"
            size="icon-sm"
            aria-label={`Editar ${row.nombre}`}
          >
            <Pencil />
          </Button>
          <Button type="button" variant="ghost" size="icon-sm" aria-label={`Eliminar ${row.nombre}`}>
            <Trash2 />
          </Button>
        </div>
      );
    }

    return row[column.id];
  }

  return (
    <DataGrid
        rows={direccionesAreaAcademica}
        columns={columns}
        getRowLabel={(row) => row.nombre}
        renderCell={renderCell}
        isEditableColumn={() => false}
        getCellEditValue={() => ""}
        applyCellEdit={(row) => row}
        getDrawerCellValue={() => null}
        canOpenDrawer={() => false}
        enableRowSelection={false}
      />
  );
}

"use client";

import {
  CalendarDays,
  Hash,
  MoreHorizontal,
  Pencil,
  Trash2,
} from "lucide-react";

import { DataGrid, type DataGridColumn } from "@/components/data-grid";
import { Button } from "@/components/ui/button";

import { periodosEscolares, type PeriodoEscolarListItem } from "../data";

type PeriodoColumnId = "codigo" | "anio" | "periodo" | "periodoMostrar" | "acciones";

type PeriodosDataGridProps = {
  onEdit: (periodo: PeriodoEscolarListItem) => void;
};

type PeriodoGridRow = PeriodoEscolarListItem;

const columns: DataGridColumn<PeriodoColumnId>[] = [
  { id: "codigo", label: "Código", icon: Hash, defaultWidth: 160 },
  { id: "anio", label: "Año de ejercicio", icon: CalendarDays, defaultWidth: 190 },
  { id: "periodo", label: "Periodo", icon: CalendarDays, defaultWidth: 190 },
  {
    id: "periodoMostrar",
    label: "Descripción del periodo",
    icon: CalendarDays,
    defaultWidth: 260,
  },
  {
    id: "acciones",
    label: "Acciones",
    icon: MoreHorizontal,
    defaultWidth: 132,
    isActions: true,
  },
];

export function PeriodosDataGrid({ onEdit }: PeriodosDataGridProps) {
  function renderCell(
    row: PeriodoGridRow,
    column: DataGridColumn<PeriodoColumnId>,
  ) {
    if (column.id === "acciones") {
      return (
        <div className="flex justify-end gap-1">
          <Button
            type="button"
            variant="ghost"
            size="icon-sm"
            aria-label={`Editar periodo ${row.codigo}`}
            onClick={() => onEdit(row)}
          >
            <Pencil />
          </Button>
          <Button
            type="button"
            variant="ghost"
            size="icon-sm"
            aria-label={`Eliminar periodo ${row.codigo}`}
          >
            <Trash2 />
          </Button>
        </div>
      );
    }

    return row[column.id];
  }

  return (
    <DataGrid
        rows={periodosEscolares}
        columns={columns}
        getRowLabel={(row) => row.codigo}
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

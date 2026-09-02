"use client";

import { AlignLeft, FileText, MoreHorizontal, Pencil, Trash2 } from "lucide-react";

import { DataGrid, type DataGridColumn } from "@/components/data-grid";
import { Button } from "@/components/ui/button";

import { articulos, type ArticuloListItem } from "../data";

type ArticuloColumnId = "numero" | "descripcion" | "acciones";

type ArticulosDataGridProps = {
  onEdit: (articulo: ArticuloListItem) => void;
};

const columns: DataGridColumn<ArticuloColumnId>[] = [
  { id: "numero", label: "Artículo", icon: FileText, defaultWidth: 220 },
  {
    id: "descripcion",
    label: "Descripción",
    icon: AlignLeft,
    defaultWidth: 460,
  },
  { id: "acciones", label: "Acciones", icon: MoreHorizontal, defaultWidth: 132 },
];

export function ArticulosDataGrid({ onEdit }: ArticulosDataGridProps) {
  function renderCell(
    row: ArticuloListItem,
    column: DataGridColumn<ArticuloColumnId>,
  ) {
    if (column.id === "acciones") {
      return (
        <div className="flex justify-end gap-1">
          <Button
            type="button"
            variant="ghost"
            size="icon-sm"
            aria-label={`Editar ${row.numero}`}
            onClick={() => onEdit(row)}
          >
            <Pencil />
          </Button>
          <Button
            type="button"
            variant="ghost"
            size="icon-sm"
            aria-label={`Eliminar ${row.numero}`}
          >
            <Trash2 />
          </Button>
        </div>
      );
    }

    return row[column.id];
  }

  return (
    <div className="overflow-hidden rounded-lg border border-border bg-background">
      <DataGrid
        rows={articulos}
        columns={columns}
        getRowLabel={(row) => row.numero}
        renderCell={renderCell}
        isEditableColumn={() => false}
        getCellEditValue={() => ""}
        applyCellEdit={(row) => row}
        getDrawerCellValue={() => null}
        canOpenDrawer={() => false}
        enableRowSelection={false}
        tableContainerClassName="overflow-hidden"
      />
    </div>
  );
}

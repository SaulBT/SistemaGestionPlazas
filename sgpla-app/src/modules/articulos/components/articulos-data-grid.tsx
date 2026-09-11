"use client";

import {
  AlignLeft,
  FileText,
  MoreHorizontal,
  Pencil,
  Trash2,
} from "lucide-react";
import { DataGrid, type DataGridColumn } from "@/components/data-grid";
import { Button } from "@/components/ui/button";
import type { Articulo } from "../domain/articulo";

type ArticuloColumnId = "numero" | "descripcion" | "acciones";
type Props = {
  articulos: Articulo[];
  isDeleting: boolean;
  onEdit: (articulo: Articulo) => void;
  onDelete: (articulo: Articulo) => void;
};
type ArticuloRow = Articulo & { id: string };

const columns: DataGridColumn<ArticuloColumnId>[] = [
  { id: "numero", label: "Artículo", icon: FileText, defaultWidth: 220 },
  {
    id: "descripcion",
    label: "Descripción",
    icon: AlignLeft,
    defaultWidth: 520,
  },
  {
    id: "acciones",
    label: "Acciones",
    icon: MoreHorizontal,
    defaultWidth: 112,
    isActions: true,
  },
];

export function ArticulosDataGrid({
  articulos,
  isDeleting,
  onEdit,
  onDelete,
}: Props) {
  const rows: ArticuloRow[] = articulos.map((articulo) => ({
    ...articulo,
    id: articulo.idArticulo.toString(),
  }));

  function renderCell(
    row: ArticuloRow,
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
            disabled={isDeleting}
            aria-label={`Eliminar ${row.numero}`}
            onClick={() => onDelete(row)}
          >
            <Trash2 className="text-destructive" />
          </Button>
        </div>
      );
    }
    return row[column.id];
  }

  return (
    <DataGrid
      rows={rows}
      columns={columns}
      getRowLabel={(row) => row.numero}
      renderCell={renderCell}
      isEditableColumn={() => false}
      getCellEditValue={() => ""}
      applyCellEdit={(row) => row}
      getDrawerCellValue={() => null}
      canOpenDrawer={() => false}
      enableRowSelection={false}
      enableFloatingActions={false}
      tableContainerClassName="overflow-hidden"
      emptyState={{
        title: "No hay artículos",
        description: "Registra el primer artículo para comenzar.",
      }}
    />
  );
}

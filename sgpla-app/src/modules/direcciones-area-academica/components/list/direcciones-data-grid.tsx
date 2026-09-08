"use client";

import Link from "next/link";
import {
  Building2,
  Eye,
  MoreHorizontal,
  Pencil,
  Phone,
  Plus,
  Trash2,
} from "lucide-react";
import {
  DataGrid,
  DataGridActions,
  type DataGridColumn,
} from "@/components/data-grid";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Skeleton } from "@/components/ui/skeleton";
import { MINIMUM_LOADING_DURATION_MS } from "@/shared/constants/loading";
import { useMinimumLoading } from "@/shared/hooks/use-minimum-loading";
import type { DireccionAreaAcademica } from "../../domain/direccion-area-academica";
import {
  toDireccionAreaAcademicaTableRow,
  type DireccionAreaAcademicaTableRow,
} from "../../presentation/direcciones-area-academica.presenter";

type DireccionColumnId = "nombre" | "telefono" | "acciones";

const columns: DataGridColumn<DireccionColumnId>[] = [
  {
    id: "nombre",
    label: "Dirección de Área Académica",
    icon: Building2,
    defaultWidth: 420,
  },
  { id: "telefono", label: "Teléfono", icon: Phone, defaultWidth: 190 },
  {
    id: "acciones",
    label: "Acciones",
    icon: MoreHorizontal,
    defaultWidth: 64,
    isActions: true,
  },
];

type Props = {
  direcciones: DireccionAreaAcademica[];
  isLoading?: boolean;
  isDeleting: boolean;
  createHref: string;
  getEditHref: (direccion: DireccionAreaAcademica) => string;
  getViewHref: (direccion: DireccionAreaAcademica) => string;
  onDelete: (direccion: DireccionAreaAcademica) => void;
};

type SkeletonRow = { id: string; nombre: string };

const skeletonRows: SkeletonRow[] = Array.from({ length: 6 }, (_, index) => ({
  id: `skeleton-${index}`,
  nombre: "Dirección de área académica",
}));

function DireccionesTableSkeleton() {
  return (
    <DataGrid
      rows={skeletonRows}
      columns={columns}
      getRowLabel={(row) => row.nombre}
      renderCell={(_, column) =>
        column.isActions ? null : <Skeleton className="h-4 w-3/4" />
      }
      isEditableColumn={() => false}
      getCellEditValue={() => ""}
      applyCellEdit={(row) => row}
      getDrawerCellValue={() => null}
      canOpenDrawer={() => false}
      enableRowSelection={false}
      enableFloatingActions={false}
      tableContainerClassName="overflow-hidden"
    />
  );
}

export function DireccionesDataGrid({
  direcciones,
  isLoading = false,
  isDeleting,
  createHref,
  getEditHref,
  getViewHref,
  onDelete,
}: Props) {
  const shouldShowLoading = useMinimumLoading(
    isLoading,
    MINIMUM_LOADING_DURATION_MS,
    0,
  );

  if (shouldShowLoading) return <DireccionesTableSkeleton />;

  const rows = direcciones.map(toDireccionAreaAcademicaTableRow);

  function renderCell(
    row: DireccionAreaAcademicaTableRow,
    column: DataGridColumn<DireccionColumnId>,
  ) {
    if (column.id === "acciones") {
      return (
        <DataGridActions>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                type="button"
                variant="ghost"
                size="icon-sm"
                aria-label={`Acciones de ${row.nombre}`}
              >
                <MoreHorizontal />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem asChild>
                <Link href={getViewHref(row.direccion)}>
                  <Eye />
                  Ver dirección
                </Link>
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <Link href={getEditHref(row.direccion)}>
                  <Pencil />
                  Editar dirección
                </Link>
              </DropdownMenuItem>
              <DropdownMenuItem
                disabled={isDeleting}
                variant="destructive"
                onSelect={() => onDelete(row.direccion)}
              >
                <Trash2 />
                Eliminar dirección
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </DataGridActions>
      );
    }

    return row[column.id];
  }

  return (
    <DataGrid
      rows={rows}
      columns={columns}
      getRowLabel={(row) => row.nombre}
      renderCell={renderCell}
      isEditableColumn={() => false}
      getCellEditValue={() => ""}
      applyCellEdit={(row) => row}
      getDrawerCellValue={() => null}
      canOpenDrawer={() => false}
      enableRowSelection={false}
      emptyState={{
        icon: <Building2 />,
        title: "No hay direcciones de áreas académicas",
        description: "Registra la primera dirección de área académica para comenzar.",
        actions: (
          <Button nativeButton={false} render={<Link href={createHref} />}>
            <Plus />
            Registrar dirección de área académica
          </Button>
        ),
      }}
    />
  );
}
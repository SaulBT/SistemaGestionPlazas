"use client";

import Link from "next/link";
import {
  University,
  Building2,
  Eye,
  GraduationCap,
  MapPin,
  Map,
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
import { useMinimumLoading } from "@/shared/hooks/use-minimum-loading";
import { MINIMUM_LOADING_DURATION_MS } from "@/shared/constants/loading";
import {
  toEntidadAcademicaTableRow,
  type EntidadAcademicaTableRow,
} from "../../presentation/entidades-academicas.presenter";
import type { EntidadAcademica } from "../../domain/entidad-academica";

type EntidadColumnId =
  | "nombre"
  | "domicilio"
  | "telefono"
  | "nombreAreaAcademica"
  | "region"
  | "acciones";

const columns: DataGridColumn<EntidadColumnId>[] = [
  {
    id: "nombre",
    label: "Entidad Académica",
    icon: University,
    defaultWidth: 260,
  },
  { id: "region", label: "Región", icon: MapPin, defaultWidth: 120 },
  { id: "domicilio", label: "Domicilio", icon: Map, defaultWidth: 360 },
  { id: "telefono", label: "Teléfono", icon: Phone, defaultWidth: 140 },
  {
    id: "nombreAreaAcademica",
    label: "Área Académica",
    icon: Building2,
    defaultWidth: 180,
  },
  {
    id: "acciones",
    label: "Acciones",
    icon: MoreHorizontal,
    defaultWidth: 64,
    isActions: true,
  },
];

type Props = {
  entidades: EntidadAcademica[];
  isLoading?: boolean;
  isDeleting: boolean;
  onDelete: (entidad: EntidadAcademica) => void;
};

type EntidadAcademicaSkeletonRow = {
  id: string;
  nombre: string;
};

const skeletonRows: EntidadAcademicaSkeletonRow[] = Array.from(
  { length: 6 },
  (_, index) => ({
    id: `skeleton-${index}`,
    nombre: "Entidad académica",
  }),
);

function EntidadesTableSkeleton() {
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

export function EntidadesDataGrid({
  entidades,
  isLoading = false,
  isDeleting,
  onDelete,
}: Props) {
  const shouldShowLoading = useMinimumLoading(
    isLoading,
    MINIMUM_LOADING_DURATION_MS,
    0,
  );

  if (shouldShowLoading) {
    return <EntidadesTableSkeleton />;
  }

  const rows = entidades.map(toEntidadAcademicaTableRow);

  function renderCell(
    row: EntidadAcademicaTableRow,
    column: DataGridColumn<EntidadColumnId>,
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
                <Link
                  href={`/EntidadesAcademicas/VerEntidadAcademica?id=${row.entidad.idEntidadAcademica}`}
                >
                  <Eye />
                  Ver entidad
                </Link>
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <Link
                  href={`/ProgramasEducativos/Buscar?idEntidadAcademica=${row.entidad.idEntidadAcademica}&region=${encodeURIComponent(row.region)}&idAreaAcademica=${row.entidad.idAreaAcademica}`}
                >
                  <GraduationCap />
                  Ver programas
                </Link>
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <Link
                  href={`/EntidadesAcademicas/EditarEntidadAcademica?id=${row.entidad.idEntidadAcademica}`}
                >
                  <Pencil />
                  Editar entidad
                </Link>
              </DropdownMenuItem>
              <DropdownMenuItem
                disabled={isDeleting}
                variant="destructive"
                onSelect={() => onDelete(row.entidad)}
              >
                <Trash2 />
                Eliminar entidad
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
        icon: <University />,
        title: "No hay entidades académicas",
        description: "Registra la primera entidad académica para comenzar.",
        actions: (
          <Button
            nativeButton={false}
            render={<Link href="/EntidadesAcademicas/CrearEntidadAcademica" />}
          >
            <Plus />
            Registrar entidad académica
          </Button>
        ),
      }}
    />
  );
}

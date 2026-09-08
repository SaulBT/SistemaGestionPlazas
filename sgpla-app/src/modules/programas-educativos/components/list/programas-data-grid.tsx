"use client";
import Link from "next/link";
import {
  Building2,
  Eye,
  GraduationCap,
  MapPin,
  MoreHorizontal,
  Pencil,
  Plus,
  Tag,
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
import type { ProgramaEducativo } from "../../domain/programa-educativo";
import {
  toProgramaEducativoTableRow,
  type ProgramaEducativoTableRow,
} from "../../presentation/programas-educativos.presenter";
type ProgramaColumnId =
  | "nombre"
  | "region"
  | "nombreAreaAcademica"
  | "nombreEntidadAcademica"
  | "acciones";
const columns: DataGridColumn<ProgramaColumnId>[] = [
  { id: "nombre", label: "Nombre", icon: GraduationCap, defaultWidth: 330 },
  { id: "region", label: "Región", icon: MapPin, defaultWidth: 190 },
  {
    id: "nombreAreaAcademica",
    label: "Área Académica",
    icon: Building2,
    defaultWidth: 270,
  },
  {
    id: "nombreEntidadAcademica",
    label: "Entidad Académica",
    icon: Building2,
    defaultWidth: 270,
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
  programas: ProgramaEducativo[];
  isLoading?: boolean;
  isDeleting: boolean;
  createHref: string;
  getEditHref: (programa: ProgramaEducativo) => string;
  getViewHref: (programa: ProgramaEducativo) => string;
  getPlansHref: (programa: ProgramaEducativo) => string;
  onDelete: (programa: ProgramaEducativo) => void;
};
const skeletonRows = Array.from({ length: 6 }, (_, index) => ({
  id: `skeleton-${index}`,
  nombre: "Programa educativo",
}));
function ProgramasTableSkeleton() {
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
export function ProgramasDataGrid({
  programas,
  isLoading = false,
  isDeleting,
  createHref,
  getEditHref,
  getViewHref,
  getPlansHref,
  onDelete,
}: Props) {
  const shouldShowLoading = useMinimumLoading(
    isLoading,
    MINIMUM_LOADING_DURATION_MS,
    0,
  );
  if (shouldShowLoading) return <ProgramasTableSkeleton />;
  const rows = programas.map(toProgramaEducativoTableRow);
  function renderCell(
    row: ProgramaEducativoTableRow,
    column: DataGridColumn<ProgramaColumnId>,
  ) {
    if (column.id === "acciones")
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
                <Link href={getViewHref(row.programa)}>
                  <Eye />
                  Ver programa
                </Link>
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <Link href={getPlansHref(row.programa)}>
                  <Tag />
                  Ver plan de estudios
                </Link>
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <Link href={getEditHref(row.programa)}>
                  <Pencil />
                  Editar programa
                </Link>
              </DropdownMenuItem>
              <DropdownMenuItem
                disabled={isDeleting}
                variant="destructive"
                onSelect={() => onDelete(row.programa)}
              >
                <Trash2 />
                Eliminar programa
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </DataGridActions>
      );
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
        icon: <GraduationCap />,
        title: "No hay programas educativos",
        description: "Registra el primer programa educativo para comenzar.",
        actions: (
          <Button nativeButton={false} render={<Link href={createHref} />}>
            <Plus />
            Registrar programa educativo
          </Button>
        ),
      }}
    />
  );
}

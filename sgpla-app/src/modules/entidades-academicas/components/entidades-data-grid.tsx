"use client";

import Link from "next/link";
import { Building2, Eye, GraduationCap, MapPin, MoreHorizontal, Pencil, Phone, Trash2 } from "lucide-react";
import { DataGrid, type DataGridColumn } from "@/components/data-grid";
import { Button } from "@/components/ui/button";
import { entidadesAcademicas, type EntidadAcademica } from "../data";

type EntidadColumnId = "nombre" | "domicilio" | "telefono" | "areaAcademica" | "region" | "acciones";
const columns: DataGridColumn<EntidadColumnId>[] = [
  { id: "nombre", label: "Nombre", icon: Building2, defaultWidth: 250 },
  { id: "domicilio", label: "Domicilio", icon: MapPin, defaultWidth: 360 },
  { id: "telefono", label: "Teléfono", icon: Phone, defaultWidth: 180 },
  { id: "areaAcademica", label: "Área Académica", icon: Building2, defaultWidth: 270 },
  { id: "region", label: "Región", icon: MapPin, defaultWidth: 210 },
  { id: "acciones", label: "Acciones", icon: MoreHorizontal, defaultWidth: 190 },
];

export function EntidadesDataGrid() {
  function renderCell(row: EntidadAcademica, column: DataGridColumn<EntidadColumnId>) {
    if (column.id === "acciones") {
      return <div className="flex justify-end gap-1">
        <Button nativeButton={false} render={<Link href={`/EntidadesAcademicas/VerEntidadAcademica?id=${row.id}`} />} variant="ghost" size="icon-sm" aria-label={`Ver ${row.nombre}`}><Eye /></Button>
        <Button nativeButton={false} render={<Link href={`/ProgramasEducativos/Buscar?idEntidadAcademica=${row.id}&region=${encodeURIComponent(row.region)}&idAreaAcademica=${encodeURIComponent(row.areaAcademica)}`} />} variant="ghost" size="icon-sm" aria-label={`Ver programas de ${row.nombre}`}><GraduationCap /></Button>
        <Button nativeButton={false} render={<Link href={`/EntidadesAcademicas/EditarEntidadAcademica?id=${row.id}`} />} variant="ghost" size="icon-sm" aria-label={`Editar ${row.nombre}`}><Pencil /></Button>
        <Button type="button" variant="ghost" size="icon-sm" aria-label={`Eliminar ${row.nombre}`}><Trash2 /></Button>
      </div>;
    }
    return row[column.id];
  }

  return <div className="overflow-hidden rounded-lg border border-border bg-background"><DataGrid rows={entidadesAcademicas} columns={columns} getRowLabel={(row) => row.nombre} renderCell={renderCell} isEditableColumn={() => false} getCellEditValue={() => ""} applyCellEdit={(row) => row} getDrawerCellValue={() => null} canOpenDrawer={() => false} enableRowSelection={false} tableContainerClassName="overflow-hidden" /></div>;
}

"use client";

import Link from "next/link";
import { Building2, GraduationCap, MapPin, MoreHorizontal, Pencil, Tag, Trash2, Eye } from "lucide-react";
import { DataGrid, type DataGridColumn } from "@/components/data-grid";
import { Button } from "@/components/ui/button";
import { programasEducativos, type ProgramaEducativo } from "../data";

type ProgramaColumnId = "nombre" | "region" | "areaAcademica" | "entidadAcademica" | "acciones";
const columns: DataGridColumn<ProgramaColumnId>[] = [
  { id: "nombre", label: "Nombre", icon: GraduationCap, defaultWidth: 330 },
  { id: "region", label: "Región", icon: MapPin, defaultWidth: 190 },
  { id: "areaAcademica", label: "Área Académica", icon: Building2, defaultWidth: 270 },
  { id: "entidadAcademica", label: "Entidad Académica", icon: Building2, defaultWidth: 270 },
  { id: "acciones", label: "Acciones", icon: MoreHorizontal, defaultWidth: 190 },
];

export function ProgramasDataGrid() {
  function renderCell(row: ProgramaEducativo, column: DataGridColumn<ProgramaColumnId>) {
    if (column.id === "acciones") return <div className="flex justify-end gap-1">
      <Button nativeButton={false} render={<Link href={`/ProgramasEducativos/VerProgramaEducativo?id=${row.id}`} />} variant="ghost" size="icon-sm" aria-label={`Ver ${row.nombre}`}><Eye /></Button>
      <Button nativeButton={false} render={<Link href={`/PlanesEstudios?programaEducativoId=${row.id}`} />} variant="ghost" size="icon-sm" aria-label={`Ver plan de estudios de ${row.nombre}`}><Tag /></Button>
      <Button nativeButton={false} render={<Link href={`/ProgramasEducativos/EditarProgramaEducativo?id=${row.id}`} />} variant="ghost" size="icon-sm" aria-label={`Editar ${row.nombre}`}><Pencil /></Button>
      <Button type="button" variant="ghost" size="icon-sm" aria-label={`Eliminar ${row.nombre}`}><Trash2 /></Button>
    </div>;
    return row[column.id];
  }

  return <div className="overflow-hidden rounded-lg border border-border bg-background"><DataGrid rows={programasEducativos} columns={columns} getRowLabel={(row) => row.nombre} renderCell={renderCell} isEditableColumn={() => false} getCellEditValue={() => ""} applyCellEdit={(row) => row} getDrawerCellValue={() => null} canOpenDrawer={() => false} enableRowSelection={false} tableContainerClassName="overflow-hidden" /></div>;
}

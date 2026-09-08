"use client";

import { FileSpreadsheet, Plus, Trash2 } from "lucide-react";
import { DataGrid, type DataGridColumn } from "@/components/data-grid";
import { FormSection } from "@/components/form-section";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { PlanEstudioFormValues } from "../../../application/programa-form.model";

type Props = {
  planesEstudio: PlanEstudioFormValues[];
  readOnly: boolean;
  onAdd: () => void;
  onChange: (index: number, changes: Partial<PlanEstudioFormValues>) => void;
  onRemove: (index: number) => void;
  errorFor: (field: string) => string | undefined;
};

type PlanGridColumn = "nombre" | "modalidad" | "archivo";
type PlanGridRow = PlanEstudioFormValues & { id: string; indice: number };

const columns: DataGridColumn<PlanGridColumn>[] = [
  {
    id: "nombre",
    label: "Plan de estudio",
    icon: FileSpreadsheet,
    defaultWidth: 240,
  },
  {
    id: "modalidad",
    label: "Modalidad",
    icon: FileSpreadsheet,
    defaultWidth: 220,
  },
  { id: "archivo", label: "Archivo", icon: FileSpreadsheet, defaultWidth: 360 },
];

export function PlanesEstudioFormSection({
  planesEstudio,
  readOnly,
  onAdd,
  onChange,
  onRemove,
  errorFor,
}: Props) {
  const rows: PlanGridRow[] = planesEstudio.map((plan, indice) => ({
    ...plan,
    id: plan.idPlanEstudios?.toString() ?? `nuevo-${indice}`,
    indice,
  }));

  function renderCell(
    row: PlanGridRow,
    column: DataGridColumn<PlanGridColumn>,
  ) {
    const prefix = `planesEstudio.${row.indice}`;
    const nombreArchivo = row.archivo?.name ?? row.nombreArchivo;

    if (column.id === "archivo") {
      return readOnly ? (
        <span className="flex items-center gap-2 text-sm text-muted-foreground">
          <FileSpreadsheet className="size-4" />
          {nombreArchivo || "Sin archivo registrado"}
        </span>
      ) : (
        <div className="space-y-1.5">
          <Label htmlFor={`${prefix}.archivo`} className="sr-only">
            Archivo
          </Label>
          <Input
            id={`${prefix}.archivo`}
            type="file"
            accept=".xls,.xlsx,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            onChange={(event) =>
              onChange(row.indice, { archivo: event.target.files?.[0] })
            }
            aria-invalid={Boolean(errorFor(`${prefix}.archivo`))}
          />
          <p className="text-xs text-muted-foreground">
            {nombreArchivo
              ? `Archivo actual: ${nombreArchivo}`
              : "Opcional · XLS/XLSX · máximo 10 MB"}
          </p>
          {errorFor(`${prefix}.archivo`) ? (
            <p className="text-xs text-destructive">
              {errorFor(`${prefix}.archivo`)}
            </p>
          ) : null}
        </div>
      );
    }

    const value = row[column.id];
    const error = errorFor(`${prefix}.${column.id}`);
    return (
      <div className="space-y-1">
        <span className={value ? undefined : "text-muted-foreground"}>
          {value || "Sin especificar"}
        </span>
        {error ? <p className="text-xs text-destructive">{error}</p> : null}
      </div>
    );
  }

  function aplicarEdicion(
    row: PlanGridRow,
    columnId: PlanGridColumn,
    nextValue: string,
  ) {
    return { ...row, [columnId]: nextValue };
  }

  function handleRowsChange(nextRows: PlanGridRow[]) {
    nextRows.forEach((row) => {
      const original = planesEstudio[row.indice];
      if (!original) return;
      if (original.nombre !== row.nombre)
        onChange(row.indice, { nombre: row.nombre });
      if (original.modalidad !== row.modalidad)
        onChange(row.indice, { modalidad: row.modalidad });
    });
  }

  return (
    <FormSection
      title="Planes de estudio"
      description="Agrega y edita los planes aplicables. La modalidad y el archivo son opcionales."
    >
      <DataGrid
        rows={rows}
        columns={columns}
        getRowLabel={(row) => row.nombre || `Plan ${row.indice + 1}`}
        renderCell={renderCell}
        isEditableColumn={(columnId) =>
          !readOnly && (columnId === "nombre" || columnId === "modalidad")
        }
        getCellEditValue={(row, columnId) => String(row[columnId] ?? "")}
        applyCellEdit={aplicarEdicion}
        getDrawerCellValue={(row, columnId) => row[columnId] ?? null}
        canOpenDrawer={() => false}
        enableRowSelection={!readOnly}
        enableFloatingActions={false}
        onRowsChange={handleRowsChange}
        renderToolbar={({
          selectedRowIds,
          selectedRowCount,
          clearSelection,
        }) =>
          !readOnly ? (
            <div className="flex flex-wrap items-center justify-between gap-2 border-b border-border bg-card p-3">
              <Button type="button" variant="outline" onClick={onAdd}>
                <Plus />
                Agregar plan de estudio
              </Button>
              <Button
                type="button"
                variant="outline"
                disabled={selectedRowCount === 0}
                onClick={() => {
                  selectedRowIds
                    .map((id) => rows.find((row) => row.id === id)?.indice)
                    .filter((indice): indice is number => indice !== undefined)
                    .sort((a, b) => b - a)
                    .forEach(onRemove);
                  clearSelection();
                }}
                className="text-destructive hover:text-destructive"
              >
                <Trash2 />
                Eliminar seleccionados
              </Button>
            </div>
          ) : null
        }
        emptyState={{
          title: "No hay planes de estudio",
          description: "Agrega un plan para asociarlo al programa educativo.",
        }}
      />
    </FormSection>
  );
}

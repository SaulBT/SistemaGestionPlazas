import * as React from "react";

import { ExternalLink } from "lucide-react";

import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { TableBody, TableCell, TableRow } from "@/components/ui/table";
import { DataGridEmptyState } from "@/components/data-grid/data-grid-empty-state";

import {
  type DataGridColumn,
  type DataGridEmptyStateProps,
  type DataGridRowBase,
  type EditingCell,
} from "@/components/data-grid/types";

type DataGridTableBodyProps<
  Row extends DataGridRowBase,
  ColumnId extends string,
> = {
  visibleRows: Row[];
  getRowLabel: (row: Row) => string;
  visibleColumns: DataGridColumn<ColumnId>[];
  editingCell: EditingCell<ColumnId> | null;
  isEditableColumn: (columnId: ColumnId) => boolean;
  startEditing: (row: Row, columnId: ColumnId) => void;
  onCellKeyDown: (
    event: React.KeyboardEvent<HTMLTableCellElement>,
    row: Row,
    rowIndex: number,
    column: DataGridColumn<ColumnId>,
    colIndex: number,
  ) => void;
  inputRef: React.RefObject<HTMLInputElement | null>;
  draftValue: string;
  setDraftValue: (value: string) => void;
  commitEdit: () => void;
  cancelEdit: () => void;
  canOpenDrawer: (columnId: ColumnId) => boolean;
  renderCell: (row: Row, column: DataGridColumn<ColumnId>) => React.ReactNode;
  onOpenDrawer: (cell: EditingCell<ColumnId>) => void;
  selectedRowIds: string[];
  onToggleRowSelection: (rowId: string, checked: boolean) => void;
  columnWidths: Record<ColumnId, number>;
  draggingColumnId: ColumnId | null;
  enableRowSelection: boolean;
  enableFloatingActions: boolean;
  emptyState?: DataGridEmptyStateProps;
};

export function DataGridTableBody<
  Row extends DataGridRowBase,
  ColumnId extends string,
>({
  visibleRows,
  getRowLabel,
  visibleColumns,
  editingCell,
  isEditableColumn,
  startEditing,
  onCellKeyDown,
  inputRef,
  draftValue,
  setDraftValue,
  commitEdit,
  cancelEdit,
  canOpenDrawer,
  renderCell,
  onOpenDrawer,
  selectedRowIds,
  onToggleRowSelection,
  columnWidths,
  draggingColumnId,
  enableRowSelection,
  enableFloatingActions,
  emptyState,
}: DataGridTableBodyProps<Row, ColumnId>) {
  return (
    <TableBody>
      {visibleRows.length === 0 ? (
        <TableRow>
          <TableCell
            colSpan={visibleColumns.length + (enableRowSelection ? 1 : 0)}
            className="h-64 p-0"
          >
            <DataGridEmptyState {...emptyState} />
          </TableCell>
        </TableRow>
      ) : null}
      {visibleRows.map((row, rowIndex) => (
        <TableRow key={row.id}>
          {enableRowSelection && (
            <TableCell className="h-10 border-r-border px-0 text-center">
              <Checkbox
                aria-label={`Select ${getRowLabel(row)}`}
                checked={selectedRowIds.includes(row.id)}
                onCheckedChange={(checked) =>
                  onToggleRowSelection(row.id, checked === true)
                }
                className="mx-auto"
              />
            </TableCell>
          )}
          {visibleColumns.map((column, colIndex) => {
            const isEditing =
              editingCell?.rowId === row.id &&
              editingCell.columnId === column.id;
            const editable = isEditableColumn(column.id);
            const showDrawerAction = canOpenDrawer(column.id);

            return (
              <TableCell
                key={`${row.id}-${column.id}`}
                data-grid-cell="true"
                data-row-index={rowIndex}
                data-col-index={colIndex}
                tabIndex={isEditing ? -1 : 0}
                className={cn(
                  "group/cell relative h-10 overflow-hidden border-r-border px-2 py-1 whitespace-nowrap outline-none",
                  "focus-visible:ring-2 focus-visible:ring-ring/40",
                  editable && "cursor-text",
                  draggingColumnId === column.id && "bg-muted/20",
                  enableFloatingActions &&
                    column.isActions &&
                    "sticky right-0 z-10 border-l-border bg-background [mask-image:linear-gradient(to_right,transparent_0,black_24px)]",
                )}
                style={{
                  width: columnWidths[column.id],
                  minWidth: columnWidths[column.id],
                }}
                onDoubleClick={() => {
                  if (editable) startEditing(row, column.id);
                }}
                onKeyDown={(event) =>
                  onCellKeyDown(event, row, rowIndex, column, colIndex)
                }
              >
                {isEditing ? (
                  <input
                    ref={inputRef}
                    value={draftValue}
                    onChange={(event) => setDraftValue(event.target.value)}
                    onBlur={commitEdit}
                    onKeyDown={(event) => {
                      if (event.key === "Enter") {
                        event.preventDefault();
                        commitEdit();
                      }

                      if (event.key === "Escape") {
                        event.preventDefault();
                        cancelEdit();
                      }
                    }}
                    className="h-7 w-full rounded-md border border-input bg-background px-2 text-xs outline-none focus-visible:ring-2 focus-visible:ring-ring/30"
                  />
                ) : (
                  <>
                    <span
                      aria-hidden
                      className="pointer-events-none absolute inset-0 border border-transparent transition-colors duration-150 group-focus-within/cell:border-ring/30"
                    />
                    <div
                      className={cn(
                        "min-w-0 truncate transition-[padding] duration-150",
                        showDrawerAction &&
                          "group-hover/cell:pr-16 group-focus-within/cell:pr-16",
                      )}
                    >
                      {renderCell(row, column)}
                    </div>
                    {showDrawerAction ? (
                      <div className="pointer-events-none absolute top-1/2 right-1.5 z-10 -translate-y-1/2 opacity-0 transition-all group-focus-within/cell:pointer-events-auto group-focus-within/cell:opacity-100 group-hover/cell:pointer-events-auto group-hover/cell:opacity-100">
                        <Button
                          type="button"
                          variant="outline"
                          size="xs"
                          aria-label={`Open details for ${getRowLabel(row)} ${column.label}`}
                          title="Open drawer"
                          className="h-7 rounded-xl border-border border bg-muted px-2.5 text-muted-foreground hover:border-ring/30 hover:bg-muted hover:text-foreground"
                          onPointerDown={(event) => {
                            event.stopPropagation();
                          }}
                          onDoubleClick={(event) => {
                            event.stopPropagation();
                          }}
                          onClick={(event) => {
                            event.stopPropagation();
                            const originElement =
                              event.currentTarget.closest("td") ??
                              event.currentTarget;
                            const originRect =
                              originElement.getBoundingClientRect();

                            onOpenDrawer({
                              rowId: row.id,
                              columnId: column.id,
                              originRect: {
                                x: originRect.x,
                                y: originRect.y,
                                width: originRect.width,
                                height: originRect.height,
                              },
                            });
                          }}
                        >
                          <ExternalLink className="size-4" />
                          Open
                        </Button>
                      </div>
                    ) : null}
                  </>
                )}
              </TableCell>
            );
          })}
        </TableRow>
      ))}
    </TableBody>
  );
}

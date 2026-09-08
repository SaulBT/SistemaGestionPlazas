"use client";

import { useState } from "react";
import { Plus } from "lucide-react";

import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";

import type { PeriodoEscolarListItem } from "../data";
import { PeriodoEscolarFormDialog } from "./periodo-escolar-form-dialog";
import { PeriodosDataGrid } from "./periodos-data-grid";
import { PeriodosFiltersHeader } from "./periodos-filters-header";
import { PeriodosPagination } from "./periodos-pagination";

export function PeriodosListView() {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingPeriodo, setEditingPeriodo] =
    useState<PeriodoEscolarListItem>();

  return (
    <Dashboard activeHref="/PeriodosEscolares">
      <div className="mx-auto max-w-7xl space-y-6 pt-24">
        <PageHeader
          title="Periodos Escolares"
          description="Administra los periodos escolares disponibles para el sistema."
          actions={
            <Button
              type="button"
              onClick={() => {
                setEditingPeriodo(undefined);
                setDialogOpen(true);
              }}
            >
              <Plus />
              Agregar Periodo Escolar
            </Button>
          }
        />
        <PeriodosFiltersHeader />
        <PeriodosDataGrid
          onEdit={(periodo) => {
            setEditingPeriodo(periodo);
            setDialogOpen(true);
          }}
        />
        <PeriodosPagination />
        <PeriodoEscolarFormDialog
          periodo={editingPeriodo}
          open={dialogOpen}
          onOpenChange={(open) => {
            setDialogOpen(open);
            if (!open) setEditingPeriodo(undefined);
          }}
        />
      </div>
    </Dashboard>
  );
}

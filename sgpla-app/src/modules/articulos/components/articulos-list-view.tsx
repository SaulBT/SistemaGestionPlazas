"use client";

import { useState } from "react";
import { Plus } from "lucide-react";

import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";

import type { ArticuloListItem } from "../data";
import { ArticuloFormDialog } from "./articulo-form-dialog";
import { ArticulosDataGrid } from "./articulos-data-grid";
import { ArticulosFiltersHeader } from "./articulos-filters-header";

export function ArticulosListView() {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingArticulo, setEditingArticulo] = useState<ArticuloListItem>();

  return (
    <Dashboard activeHref="/Articulos">
      <div className="mx-auto max-w-7xl space-y-6 pt-24">
        <PageHeader
          title="Artículos"
          description="Administra los artículos y disposiciones del sistema."
          actions={
            <Button
              type="button"
              onClick={() => {
                setEditingArticulo(undefined);
                setDialogOpen(true);
              }}
            >
              <Plus />
              Agregar Artículo
            </Button>
          }
        />
        <ArticulosFiltersHeader />
        <ArticulosDataGrid
          onEdit={(articulo) => {
            setEditingArticulo(articulo);
            setDialogOpen(true);
          }}
        />
        <ArticuloFormDialog
          articulo={editingArticulo}
          open={dialogOpen}
          onOpenChange={(open) => {
            setDialogOpen(open);
            if (!open) setEditingArticulo(undefined);
          }}
        />
      </div>
    </Dashboard>
  );
}

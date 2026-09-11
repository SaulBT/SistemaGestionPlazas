"use client";

import { useState } from "react";
import { Plus } from "lucide-react";
import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { Button } from "@/components/ui/button";
import { ConfirmationDialog } from "@/components/ui/confirmation-dialog";
import { toast } from "@/components/ui/toast";
import { getErrorMessage } from "@/shared/api/http-client";
import type { Articulo } from "../domain/articulo";
import { useArticulosListController } from "../presentation/hooks/use-articulos-list-controller";
import { ArticuloFormDialog } from "./articulo-form-dialog";
import { ArticulosDataGrid } from "./articulos-data-grid";
import { ArticulosFiltersHeader } from "./articulos-filters-header";

export function ArticulosListView() {
  const controller = useArticulosListController();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingArticulo, setEditingArticulo] = useState<Articulo>();

  function openCreate() {
    setEditingArticulo(undefined);
    setDialogOpen(true);
  }
  function openEdit(articulo: Articulo) {
    setEditingArticulo(articulo);
    setDialogOpen(true);
  }

  return (
    <Dashboard activeHref="/Articulos">
      <div className="mx-auto max-w-7xl space-y-6 pt-24">
        <PageHeader
          title="Artículos"
          description="Administra los artículos y disposiciones del sistema."
          actions={
            <Button type="button" onClick={openCreate}>
              <Plus />
              Agregar artículo
            </Button>
          }
        />
        <ArticulosFiltersHeader
          value={controller.busqueda}
          onChange={controller.setBusqueda}
        />
        {controller.articulosQuery.isError ? (
          <p className="text-sm font-medium text-destructive" role="alert">
            {getErrorMessage(controller.articulosQuery.error)}
          </p>
        ) : null}
        <ArticulosDataGrid
          articulos={controller.articulos}
          isDeleting={controller.isDeleting}
          onEdit={openEdit}
          onDelete={controller.setPendiente}
        />
        <ArticuloFormDialog
          key={`${editingArticulo?.idArticulo ?? "nuevo"}-${dialogOpen}`}
          articulo={editingArticulo}
          open={dialogOpen}
          onOpenChange={(open) => {
            setDialogOpen(open);
            if (!open) setEditingArticulo(undefined);
          }}
          onSaved={(editing) =>
            toast.add({
              type: "success",
              title: editing ? "Artículo actualizado" : "Artículo registrado",
              description: "Los cambios se guardaron correctamente.",
            })
          }
        />
      </div>
      <ConfirmationDialog
        open={Boolean(controller.pendiente)}
        onOpenChange={(open) => {
          if (!open) controller.setPendiente(null);
        }}
        title="¿Eliminar artículo?"
        description={
          controller.pendiente
            ? `Se eliminará “${controller.pendiente.numero}”. Esta acción no se puede deshacer.`
            : "Esta acción no se puede deshacer."
        }
        confirmLabel="Eliminar artículo"
        destructive
        onConfirm={() => {
          void controller
            .confirmarEliminacion()
            .then(() =>
              toast.add({
                type: "success",
                title: "Artículo eliminado",
                description: "El artículo se eliminó correctamente.",
              }),
            )
            .catch(() =>
              toast.add({
                type: "error",
                title: "No se pudo eliminar",
                description: "Intenta nuevamente.",
              }),
            );
        }}
      />
    </Dashboard>
  );
}

"use client";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Field, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";

import type { ArticuloListItem } from "../data";

type ArticuloFormDialogProps = {
  articulo?: ArticuloListItem;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function ArticuloFormDialog({
  articulo,
  open,
  onOpenChange,
}: ArticuloFormDialogProps) {
  const isEditing = Boolean(articulo);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Artículo</DialogTitle>
          <DialogDescription>
            {isEditing
              ? "Actualiza los datos del artículo."
              : "Registra un nuevo artículo."}
          </DialogDescription>
        </DialogHeader>

        <form key={articulo?.id ?? "nuevo"} className="grid gap-4">
          <Field>
            <FieldLabel htmlFor="numero">Título</FieldLabel>
            <Input
              id="numero"
              name="numero"
              defaultValue={articulo?.numero}
              placeholder="Ej. Artículo 1"
              maxLength={50}
            />
          </Field>

          <Field>
            <FieldLabel htmlFor="descripcion">Descripción</FieldLabel>
            <Textarea
              id="descripcion"
              name="descripcion"
              defaultValue={articulo?.descripcion}
              placeholder="Describe el artículo..."
              maxLength={250}
            />
          </Field>
        </form>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
          >
            Cancelar
          </Button>
          <Button type="button" onClick={() => onOpenChange(false)}>
            {isEditing ? "Guardar cambios" : "Guardar"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

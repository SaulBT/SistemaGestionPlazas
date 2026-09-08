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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { periodosOptions, type PeriodoEscolarListItem } from "../data";

type PeriodoEscolarFormDialogProps = {
  periodo?: PeriodoEscolarListItem;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function PeriodoEscolarFormDialog({
  periodo,
  open,
  onOpenChange,
}: PeriodoEscolarFormDialogProps) {
  const isEditing = Boolean(periodo);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Periodo Escolar</DialogTitle>
          <DialogDescription>
            {isEditing
              ? "Actualiza los datos del periodo escolar."
              : "Registra un nuevo periodo escolar."}
          </DialogDescription>
        </DialogHeader>

        <form key={periodo?.id ?? "nuevo"} className="grid gap-4">
          <Field>
            <FieldLabel htmlFor="anio">Año de ejercicio</FieldLabel>
            <Input
              id="anio"
              name="anio"
              defaultValue={periodo?.anio}
              inputMode="numeric"
              maxLength={4}
              placeholder="Ej. 2025"
            />
          </Field>

          <Field>
            <FieldLabel htmlFor="periodo">Periodo</FieldLabel>
            <Select name="periodo" defaultValue={periodo?.periodo}>
              <SelectTrigger id="periodo">
                <SelectValue placeholder="Seleccione un periodo" />
              </SelectTrigger>
              <SelectContent>
                {periodosOptions.map((option) => (
                  <SelectItem key={option} value={option}>
                    {option}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
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

import * as React from "react";
import { Table2 } from "lucide-react";

import {
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import type { DataGridEmptyStateProps } from "@/components/data-grid/types";

export function DataGridEmptyState({
  icon = <Table2 />,
  title = "No hay registros",
  description = "No existen datos para mostrar en esta tabla.",
  actions,
}: DataGridEmptyStateProps) {
  return (
    <Empty className="min-h-56 border-0">
      <EmptyHeader>
        <EmptyMedia variant="icon">{icon}</EmptyMedia>
        <EmptyTitle>{title}</EmptyTitle>
        {description ? <EmptyDescription>{description}</EmptyDescription> : null}
      </EmptyHeader>
      {actions ? <EmptyContent>{actions}</EmptyContent> : null}
    </Empty>
  );
}

import * as React from "react";

import { cn } from "@/lib/utils";

type DataGridActionsProps = {
  children: React.ReactNode;
  className?: string;
};

export function DataGridActions({
  children,
  className,
}: DataGridActionsProps) {
  return (
    <div className={cn("flex w-full justify-end gap-1", className)}>
      {children}
    </div>
  );
}

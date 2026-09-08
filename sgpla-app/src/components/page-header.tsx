"use client";

import { usePathname, useRouter } from "next/navigation";
import { ArrowLeft } from "lucide-react";
import type { ReactNode } from "react";
import { Button } from "@/components/ui/button";

type PageHeaderProps = {
  title: string;
  description?: string;
  actions?: ReactNode;
  isRoot?: boolean;
  onBack?: () => void;
};

export function PageHeader({
  title,
  description,
  actions,
  isRoot = false,
  onBack,
}: PageHeaderProps) {
  const router = useRouter();
  const pathname = usePathname();
  const routeSegments = pathname.split("/").filter(Boolean);
  const parentPath =
    routeSegments.length > 1
      ? `/${routeSegments.slice(0, -1).join("/")}`
      : null;
  const backDisabled = isRoot || parentPath === null;

  return (
    <header
      className="fixed inset-x-0 top-21 z-10 h-32 mask-[linear-gradient(to_bottom,#000_0%,#000_58%,transparent_100%)] bg-background md:left-(--sidebar-offset)"
      aria-label={`Encabezado de ${title}`}
    >
      <div className="flex h-16 items-center justify-between gap-6 px-6 pt-4 md:px-8">
        <div className="flex min-w-0 items-center gap-4">
          <Button
            type="button"
            size="icon"
            aria-label="Volver a la página anterior"
            variant="secondary"
            disabled={backDisabled}
            onClick={() =>
              onBack ? onBack() : parentPath && router.push(parentPath)
            }
          >
            <ArrowLeft />
          </Button>
          <div className="min-w-0">
            <h1 className="truncate text-2xl font-semibold leading-snug tracking-tight">
              {title}
            </h1>
            {description && (
              <p className="text-sm text-muted-foreground">{description}</p>
            )}
          </div>
        </div>
        {actions && (
          <div className="flex shrink-0 items-center gap-2">{actions}</div>
        )}
      </div>
    </header>
  );
}

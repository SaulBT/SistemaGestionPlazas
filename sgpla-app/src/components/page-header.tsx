import type { ReactNode } from "react";

type PageHeaderProps = {
  title: string;
  description?: string;
  actions?: ReactNode;
};

export function PageHeader({ title, description, actions }: PageHeaderProps) {
  return (
    <header
      className="fixed inset-x-0 top-14 z-10 h-32 mask-[linear-gradient(to_bottom,#000_0%,#000_58%,transparent_100%)] bg-background md:left-(--sidebar-offset)"
      aria-label={`Encabezado de ${title}`}
    >
      <div className="flex h-16 items-center justify-between gap-6 px-6 pt-4 md:px-8">
        <div className="min-w-0">
          <h1 className="truncate text-2xl font-semibold leading-snug tracking-tight">
            {title}
          </h1>
          {description && (
            <p className="text-sm text-muted-foreground">{description}</p>
          )}
        </div>
        {actions && (
          <div className="flex shrink-0 items-center gap-2">{actions}</div>
        )}
      </div>
    </header>
  );
}

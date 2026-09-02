import { SidebarTrigger } from "@/components/ui/sidebar";

export function DashboardTopBar() {
  return (
    <header
      className="fixed inset-x-0 top-0 z-20 h-20 mask-[linear-gradient(to_bottom,#000_0%,#000_65%,transparent_100%)] bg-background md:left-(--sidebar-offset)"
      aria-label="Barra superior del dashboard"
    >
      <div className="flex h-14 items-center gap-3 px-6 md:px-8">
        <SidebarTrigger />
        <div>
          <p className="text-sm font-medium">
            Sistema de Gestión de Plazas Vacantes
          </p>
          <p className="text-xs text-muted-foreground">
            Panel de administración
          </p>
        </div>
      </div>
    </header>
  );
}

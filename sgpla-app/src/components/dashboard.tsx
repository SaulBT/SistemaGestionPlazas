"use client";

import Link from "next/link";
import {
  BookOpen,
  Building2,
  CalendarDays,
  FileText,
  GraduationCap,
  LayoutDashboard,
  Library,
  Users,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";

import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarSeparator,
  SidebarTrigger,
} from "@/components/ui/sidebar";

type DashboardItem = {
  label: string;
  href: string;
  icon: LucideIcon;
};

const navigation: DashboardItem[] = [
  { label: "Usuarios", href: "/usuarios", icon: Users },
  { label: "Artículos", href: "/articulos", icon: FileText },
  { label: "Direcciones de área", href: "/direcciones-area-academica", icon: Building2 },
  { label: "Entidades académicas", href: "/entidades-academicas", icon: Library },
  { label: "Programas educativos", href: "/programas-educativos", icon: GraduationCap },
  { label: "Planes de estudio", href: "/planes-estudios", icon: BookOpen },
  { label: "Periodos escolares", href: "/periodos-escolares", icon: CalendarDays },
];

function NavigationItem({ item, active = false }: { item: DashboardItem; active?: boolean }) {
  return (
    <SidebarMenuItem>
      <SidebarMenuButton
        render={<Link href={item.href} />}
        isActive={active}
        icon={item.icon}
      >
        {item.label}
      </SidebarMenuButton>
    </SidebarMenuItem>
  );
}

export function Dashboard() {
  return (
    <SidebarProvider>
      <Sidebar>
        <SidebarHeader>
          <SidebarMenu>
            <SidebarMenuItem>
              <SidebarMenuButton render={<Link href="/" />} icon={LayoutDashboard} size="lg" isActive>
                SGPla
              </SidebarMenuButton>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarHeader>

        <SidebarSeparator />

        <SidebarContent>
          <SidebarGroup>
            <SidebarGroupLabel>Administración</SidebarGroupLabel>
            <SidebarMenu>
              {navigation.map((item) => <NavigationItem key={item.href} item={item} />)}
            </SidebarMenu>
          </SidebarGroup>
        </SidebarContent>

        <SidebarFooter>
          <SidebarSeparator />
          <SidebarMenu>
            <SidebarMenuItem>
              <SidebarMenuButton render={<Link href="/login" />} icon={Users}>
                Cerrar sesión
              </SidebarMenuButton>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarFooter>
      </Sidebar>

      <SidebarInset>
        <header className="flex h-14 items-center gap-3 border-b border-border px-4">
          <SidebarTrigger />
          <div>
            <p className="text-sm font-medium">Sistema de Gestión de Plazas Vacantes</p>
            <p className="text-xs text-muted-foreground">Panel de administración</p>
          </div>
        </header>

        <main className="flex-1 bg-[#f5f5f5] p-6 md:p-8">
          <div className="mx-auto max-w-6xl">
            <div className="mb-8">
              <p className="text-sm text-muted-foreground">SuperUsuario</p>
              <h1 className="mt-1 text-3xl font-semibold tracking-tight">Panel principal</h1>
              <p className="mt-2 text-muted-foreground">
                Selecciona una opción para comenzar a gestionar el sistema.
              </p>
            </div>

            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {navigation.map((item) => {
                const Icon = item.icon;
                return (
                  <Link
                    key={item.href}
                    href={item.href}
                    className="group flex min-h-32 flex-col justify-between rounded-xl border border-border bg-background p-5 transition-colors hover:bg-accent"
                  >
                    <Icon className="size-5 text-muted-foreground transition-colors group-hover:text-foreground" />
                    <span className="font-medium">{item.label}</span>
                  </Link>
                );
              })}
            </div>
          </div>
        </main>
      </SidebarInset>
    </SidebarProvider>
  );
}

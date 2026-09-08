import { Dashboard } from "@/components/dashboard";
import { PageHeader } from "@/components/page-header";
import { UserRegistrationForm } from "@/modules/users";
import Link from "next/link";

export const metadata = {
  title: "Crear Usuario - SGPla",
};

export default function CreateUserPage() {
  return (
    <Dashboard activeHref="/Usuarios">
      <div className="mx-auto max-w-5xl space-y-6 pt-24">
        <PageHeader
          title="Crear Usuario"
          description="Registra un nuevo usuario y asigna su rol dentro del sistema."
          actions={
            <Link
              href="/Usuarios"
              className="inline-flex h-9 items-center justify-center rounded-md bg-accent px-4 text-[13px] font-medium text-foreground shadow-sm transition-colors hover:bg-accent/80 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            >
              Regresar
            </Link>
          }
        />

        <section className="rounded-lg border border-border bg-background p-6 md:p-8">
          <UserRegistrationForm />
        </section>
      </div>
    </Dashboard>
  );
}

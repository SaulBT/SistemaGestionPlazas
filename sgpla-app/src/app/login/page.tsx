import { Button } from "@/components/ui/button";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";

export const metadata = {
  title: "Inicio de sesión - SGPla",
};

export default function LoginPage() {
  return (
    <>
      <header className="fixed inset-x-0 top-0 z-10 flex h-15 items-center bg-[#005baa] px-5 text-white shadow-[0_2px_5px_rgba(0,0,0,0.1)]">
        <div className="text-xl font-bold">
          Sistema de Gestión de Plazas Vacantes
        </div>
      </header>

      <main className="min-h-screen bg-[#f5f5f5] px-5 pt-25">
        <section className="mx-auto w-full max-w-125 rounded-[10px] bg-white p-7.5 shadow-[0_2px_10px_rgba(0,0,0,0.05)]">
          <div className="mb-3 border-b border-[#ddd] pb-2.5">
            <div className="flex items-center justify-between pt-2.5">
              <h1 className="mb-0 text-xl font-medium">Inicio de sesión</h1>
            </div>

            <nav aria-label="breadcrumb" className="mt-2">
              <ol className="flex gap-2 text-sm text-muted-foreground">
                <li>Inicio</li>
                <li aria-hidden="true">/</li>
                <li aria-current="page">Login</li>
              </ol>
            </nav>
          </div>

          <form method="post">
            <FieldGroup>
              <Field>
                <FieldLabel htmlFor="correo">Correo:</FieldLabel>
                <Input id="correo" name="Correo" type="text" />
              </Field>

              <Field>
                <FieldLabel htmlFor="contrasena">Contraseña:</FieldLabel>
                <Input
                  id="contrasena"
                  name="Contrasena"
                  type="password"
                  autoComplete="current-password"
                />
              </Field>
            </FieldGroup>

            <div className="mt-4 flex justify-end">
              <Button type="submit">Iniciar sesión</Button>
            </div>
          </form>
        </section>
      </main>
    </>
  );
}

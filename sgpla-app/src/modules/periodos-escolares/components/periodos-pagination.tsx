import Link from "next/link";

import { Button } from "@/components/ui/button";

export function PeriodosPagination() {
  return (
    <nav
      aria-label="Paginación de periodos escolares"
      className="flex items-center justify-end gap-2 text-sm"
    >
      <span className="mr-2 text-muted-foreground">Página 1 de 1</span>
      <Button
        nativeButton={false}
        render={<Link href="/PeriodosEscolares?pagina=1&cantidad=10" />}
        size="sm"
        variant="secondary"
      >
        Anterior
      </Button>
      <Button
        nativeButton={false}
        render={<Link href="/PeriodosEscolares?pagina=1&cantidad=10" />}
        size="sm"
        variant="secondary"
      >
        Siguiente
      </Button>
    </nav>
  );
}

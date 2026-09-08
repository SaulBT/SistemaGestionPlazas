"use client";

import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination";
import { buildPageItems } from "@/shared/pagination/build-page-items";

type Props = {
  currentPage: number;
  isLoading: boolean;
  totalPages: number;
  onPageChange: (page: number) => void;
};

export function DireccionesPagination({
  currentPage,
  isLoading,
  totalPages,
  onPageChange,
}: Props) {
  const pageItems = buildPageItems(currentPage, totalPages);

  return (
    <div className="flex items-center justify-end gap-4 text-sm text-muted-foreground">
      <span className="whitespace-nowrap">
        Página {currentPage} de {totalPages}
      </span>
      <Pagination className="mx-0 w-auto">
        <PaginationContent>
          <PaginationItem>
            <PaginationPrevious
              href="#"
              disabled={currentPage <= 1 || isLoading}
              onClick={(event) => {
                event.preventDefault();
                onPageChange(currentPage - 1);
              }}
            />
          </PaginationItem>
          {pageItems.map((item, index) =>
            item === "ellipsis" ? (
              <PaginationItem key={`ellipsis-${index}`}>
                <PaginationEllipsis />
              </PaginationItem>
            ) : (
              <PaginationItem key={item}>
                <PaginationLink
                  href="#"
                  isActive={item === currentPage}
                  disabled={isLoading}
                  onClick={(event) => {
                    event.preventDefault();
                    onPageChange(item);
                  }}
                >
                  {item}
                </PaginationLink>
              </PaginationItem>
            ),
          )}
          <PaginationItem>
            <PaginationNext
              href="#"
              disabled={currentPage >= totalPages || isLoading}
              onClick={(event) => {
                event.preventDefault();
                onPageChange(currentPage + 1);
              }}
            />
          </PaginationItem>
        </PaginationContent>
      </Pagination>
    </div>
  );
}
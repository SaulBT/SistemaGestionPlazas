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

type PaginationItemValue = number | "ellipsis";

type Props = {
  currentPage: number;
  isLoading: boolean;
  totalPages: number;
  onPageChange: (page: number) => void;
};

function getPageItems(
  currentPage: number,
  totalPages: number,
): PaginationItemValue[] {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, index) => index + 1);
  }

  if (currentPage <= 4) {
    return [1, 2, 3, 4, 5, "ellipsis", totalPages];
  }

  if (currentPage >= totalPages - 3) {
    return [
      1,
      "ellipsis",
      totalPages - 4,
      totalPages - 3,
      totalPages - 2,
      totalPages - 1,
      totalPages,
    ];
  }

  return [
    1,
    "ellipsis",
    currentPage - 1,
    currentPage,
    currentPage + 1,
    "ellipsis",
    totalPages,
  ];
}

export function EntidadesPagination({
  currentPage,
  isLoading,
  totalPages,
  onPageChange,
}: Props) {
  const pageItems = getPageItems(currentPage, totalPages);

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

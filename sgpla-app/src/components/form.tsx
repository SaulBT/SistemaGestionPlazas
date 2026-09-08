import type { ComponentProps } from "react";

import { cn } from "@/lib/utils";

function Form({ className, ...props }: ComponentProps<"form">) {
  return (
    <form
      className={cn("mx-auto max-w-2xl space-y-8 pt-6", className)}
      {...props}
    />
  );
}

function FormContent({ className, ...props }: ComponentProps<"div">) {
  return <div className={cn("flex flex-col gap-10", className)} {...props} />;
}

export { Form, FormContent };

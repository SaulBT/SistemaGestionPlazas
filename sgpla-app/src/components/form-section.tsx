import type { ReactNode } from "react";

type FormSectionProps = {
  title: ReactNode;
  description?: ReactNode;
  children: ReactNode;
};

export function FormSection({
  title,
  description,
  children,
}: FormSectionProps) {
  return (
    <fieldset className="m-0 grid min-w-0 gap-x-20 border-0 p-0 md:grid-cols-[180px_minmax(0,1fr)]">
      <legend className="contents">
        <div className="flex flex-col gap-1 md:col-start-1 md:row-start-1">
          <span className="text-base font-medium text-foreground">{title}</span>
          {description ? (
            <p className="text-sm text-muted-foreground">{description}</p>
          ) : null}
        </div>
      </legend>
      <div className="flex w-full max-w-2xl min-w-0 flex-col gap-5 md:col-start-2 md:row-start-1">
        {children}
      </div>
    </fieldset>
  );
}

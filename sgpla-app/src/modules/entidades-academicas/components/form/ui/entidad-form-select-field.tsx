import { FieldError } from "@/components/ui/field";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { TextField } from "@/components/text-field";

type Props = {
  id: "idAreaAcademica" | "region";
  label: string;
  value?: string;
  options: Array<{ value: string; label: string }>;
  onChange: (value: string) => void;
  readOnly?: boolean;
  loading?: boolean;
  error?: string;
};

export function EntidadFormSelectField({
  id,
  label,
  value,
  options,
  onChange,
  readOnly = false,
  loading = false,
  error,
}: Props) {
  return (
    <TextField id={id} required>
      <Label>{label}</Label>
      <Select
        name={id}
        value={value}
        onValueChange={onChange}
        disabled={readOnly || loading}
      >
        <SelectTrigger id={id} className="w-full" aria-invalid={Boolean(error)}>
          <SelectValue
            placeholder={
              loading
                ? "Cargando opciones..."
                : `Seleccione ${label.toLowerCase()}`
            }
          />
        </SelectTrigger>
        <SelectContent>
          {options.map((option) => (
            <SelectItem key={option.value} value={option.value}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      <FieldError>{error}</FieldError>
    </TextField>
  );
}

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

export type SelectOption = { value: string; label: string };

type Props = {
  id: string;
  label: string;
  options: SelectOption[];
  value?: string;
  onChange: (value: string) => void;
  placeholder?: string;
  disabled?: boolean;
  loading?: boolean;
  error?: string;
  required?: boolean;
};

export function FormSelectField({
  id,
  label,
  options,
  value,
  onChange,
  placeholder = `Seleccione ${label.toLowerCase()}`,
  disabled = false,
  loading = false,
  error,
  required = false,
}: Props) {
  return (
    <TextField id={id} required={required}>
      <Label>{label}</Label>
      <Select
        name={id}
        value={value}
        onValueChange={onChange}
        disabled={disabled || loading}
      >
        <SelectTrigger id={id} className="w-full" aria-invalid={Boolean(error)}>
          <SelectValue placeholder={loading ? "Cargando opciones..." : placeholder} />
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

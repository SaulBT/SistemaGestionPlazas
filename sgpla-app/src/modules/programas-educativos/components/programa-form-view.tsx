import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Field, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { areasAcademicas, entidadesAcademicas, regiones, type ProgramaEducativo } from "../data";

type Props = { programa?: ProgramaEducativo; readOnly?: boolean };
function TextField({ id, label, value, maxLength, readOnly = false }: { id: string; label: string; value?: string; maxLength?: number; readOnly?: boolean }) { return <Field><FieldLabel htmlFor={id}>{label}</FieldLabel><Input id={id} name={id} defaultValue={value} maxLength={maxLength} disabled={readOnly} /></Field>; }
function SelectField({ id, label, value, options, readOnly = false }: { id: string; label: string; value?: string; options: string[]; readOnly?: boolean }) { return <Field><FieldLabel htmlFor={id}>{label}</FieldLabel><Select name={id} defaultValue={value} disabled={readOnly}><SelectTrigger id={id} className="w-full"><SelectValue placeholder={`Seleccione ${label.toLowerCase()}`} /></SelectTrigger><SelectContent>{options.map((option) => <SelectItem key={option} value={option}>{option}</SelectItem>)}</SelectContent></Select></Field>; }

export function ProgramaFormView({ programa, readOnly = false }: Props) {
  return <div className="mx-auto max-w-5xl space-y-8 pt-24"><div className="grid gap-6"><div className="grid gap-6 sm:grid-cols-[minmax(8rem,1fr)_3fr]"><TextField id="clave" label="Clave" value={programa?.clave} maxLength={5} readOnly={readOnly} /><TextField id="nombre" label="Nombre" value={programa?.nombre} maxLength={94} readOnly={readOnly} /></div><div className="grid gap-6 sm:grid-cols-2"><TextField id="campus" label="Campus" value={programa?.campus} maxLength={100} readOnly={readOnly} /><SelectField id="region" label="Región" value={programa?.region} options={regiones} readOnly={readOnly} /></div><div className="grid gap-6 sm:grid-cols-2"><SelectField id="idAreaAcademica" label="Área Académica" value={programa?.areaAcademica} options={areasAcademicas} readOnly={readOnly} /><SelectField id="idEntidadAcademica" label="Entidad Académica" value={programa?.entidadAcademica} options={entidadesAcademicas} readOnly={readOnly} /></div></div><div className="flex justify-end gap-2"><Button nativeButton={false} render={<Link href="/ProgramasEducativos" />} variant="outline">{readOnly ? "Regresar" : "Cancelar"}</Button>{!readOnly && <Button type="button">Guardar</Button>}</div></div>;
}

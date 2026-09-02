import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Field, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { areasAcademicas, regiones, type EntidadAcademica } from "../data";

type Props = { entidad?: EntidadAcademica; readOnly?: boolean };

function TextField({ id, label, value, maxLength, readOnly = false }: { id: string; label: string; value?: string; maxLength?: number; readOnly?: boolean }) {
  return <Field><FieldLabel htmlFor={id}>{label}</FieldLabel><Input id={id} name={id} defaultValue={value} maxLength={maxLength} disabled={readOnly} /></Field>;
}

function SelectField({ id, label, value, options, readOnly = false }: { id: string; label: string; value?: string; options: string[]; readOnly?: boolean }) {
  return <Field><FieldLabel htmlFor={id}>{label}</FieldLabel><Select name={id} defaultValue={value} disabled={readOnly}><SelectTrigger id={id} className="w-full"><SelectValue placeholder={`Seleccione ${label.toLowerCase()}`} /></SelectTrigger><SelectContent>{options.map((option) => <SelectItem key={option} value={option}>{option}</SelectItem>)}</SelectContent></Select></Field>;
}

export function EntidadFormView({ entidad, readOnly = false }: Props) {
  return <div className="mx-auto max-w-5xl space-y-8 pt-24"><div className="grid gap-6">
    <div className="grid gap-6 sm:grid-cols-[minmax(8rem,1fr)_3fr]"><TextField id="clave" label="Clave" value={entidad?.clave} maxLength={5} readOnly={readOnly} /><TextField id="nombre" label="Nombre" value={entidad?.nombre} maxLength={94} readOnly={readOnly} /></div>
    <div className="grid gap-6 sm:grid-cols-[2fr_1fr_0.6fr_1fr]"><TextField id="calleNumero" label="Domicilio (Calle y Número)" value={entidad?.calleNumero} maxLength={150} readOnly={readOnly} /><TextField id="colonia" label="Colonia" value={entidad?.colonia} maxLength={100} readOnly={readOnly} /><TextField id="cp" label="C.P." value={entidad?.cp} maxLength={5} readOnly={readOnly} /><TextField id="municipio" label="Municipio" value={entidad?.municipio} maxLength={100} readOnly={readOnly} /></div>
    <div className="grid gap-6 sm:grid-cols-2"><TextField id="telefono" label="Teléfono" value={entidad?.telefono} maxLength={30} readOnly={readOnly} /><TextField id="extension" label="Extensión" value={entidad?.extension} maxLength={5} readOnly={readOnly} /></div>
    <div className="grid gap-6 sm:grid-cols-2"><SelectField id="idAreaAcademica" label="Área Académica" value={entidad?.areaAcademica} options={areasAcademicas} readOnly={readOnly} /><SelectField id="region" label="Región" value={entidad?.region} options={regiones} readOnly={readOnly} /></div>
  </div><div className="flex justify-end gap-2"><Button nativeButton={false} render={<Link href="/EntidadesAcademicas" />} variant="outline">{readOnly ? "Regresar" : "Cancelar"}</Button>{!readOnly && <Button type="button">Guardar</Button>}</div></div>;
}

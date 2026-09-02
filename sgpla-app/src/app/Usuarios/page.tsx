import { Dashboard } from "@/components/dashboard";
import { UsersListView } from "@/modules/users";

export const metadata = {
  title: "Usuarios - SGPla",
};

export default function UsersPage() {
  return (
    <Dashboard activeHref="/Usuarios">
      <UsersListView />
    </Dashboard>
  );
}

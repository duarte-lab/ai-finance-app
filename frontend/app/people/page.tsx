import { redirectToSignInIfUnauthorized, requireBackendToken } from "@/lib/session";
import { getPeople } from "@/services/api";
import { PeopleManager } from "../../components/PeopleManager";

export const dynamic = "force-dynamic";

export default async function PeoplePage() {
  const token = await requireBackendToken();

  try {
    const people = await getPeople(token);
    return <PeopleManager initialPeople={people} token={token} />;
  } catch (error) {
    redirectToSignInIfUnauthorized(error);
  }

  return null;
}

import { getPeople } from "@/services/api";
import { PeopleManager } from "../../components/PeopleManager";

export const dynamic = "force-dynamic";

export default async function PeoplePage() {
  const people = await getPeople();

  return <PeopleManager initialPeople={people} />;
}

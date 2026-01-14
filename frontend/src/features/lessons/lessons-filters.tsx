import { Input } from "@/shared/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Search } from "lucide-react";
import { useState } from "react";
import { useDebounce } from "use-debounce";

export function LessonFilters() {
  const [search, setSearch] = useState("");
  const [isDeleted, setIsDeleted] = useState<boolean | undefined>(false);

  const [debouncedSearch] = useDebounce(search, 300);
  return (
    <>
      <div className="relative flex-1 w-100">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4" />
        <Input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="pl-9"
          placeholder="Поиск по названию"
        />
      </div>
      <Select
        value={
          isDeleted === undefined ? "all" : isDeleted ? "deleted" : "active"
        }
        onValueChange={(value) => {
          if (value === "all") {
            setIsDeleted(undefined);
          } else if (value) {
            setIsDeleted(true);
          } else {
            setIsDeleted(false);
          }
        }}
      >
        <SelectTrigger className="w-45">
          <SelectValue placeholder="Статус" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">Все</SelectItem>
          <SelectItem value="active">Активные</SelectItem>
          <SelectItem value="deleted">Удаленные</SelectItem>
        </SelectContent>
      </Select>
    </>
  );
}

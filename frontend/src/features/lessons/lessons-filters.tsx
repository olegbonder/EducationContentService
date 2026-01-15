import { Input } from "@/shared/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { Search } from "lucide-react";
import { useEffect, useState } from "react";
import { useDebounce } from "use-debounce";
import {
  setFilterIsDeleted,
  setFilterSearch,
  useGetLessonFilter,
} from "./model/lessons-filters-store";

export function LessonsFilters() {
  const { search, isDeleted } = useGetLessonFilter();

  const [localSearch, setLocalSearch] = useState<string>(search ?? "");
  const [debouncedSearch] = useDebounce(localSearch, 300);

  useEffect(() => {
    setFilterSearch(debouncedSearch);
  }, [debouncedSearch]);

  return (
    <div className="flex gap-4 mb-4">
      <div className="relative flex-1">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          value={localSearch}
          onChange={(e) => setLocalSearch(e.target.value)}
          placeholder="Поиск по названию..."
          className="pl-9"
        />
      </div>
      <Select
        value={
          isDeleted === undefined ? "all" : isDeleted ? "deleted" : "active"
        }
        onValueChange={(value) => {
          if (value === "all") setFilterIsDeleted(undefined);
          else if (value === "deleted") setFilterIsDeleted(true);
          else setFilterIsDeleted(false);
        }}
      >
        <SelectTrigger className="w-[180px]">
          <SelectValue placeholder="Статус" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">Все</SelectItem>
          <SelectItem value="active">Активные</SelectItem>
          <SelectItem value="deleted">Удалённые</SelectItem>
        </SelectContent>
      </Select>
    </div>
  );
}

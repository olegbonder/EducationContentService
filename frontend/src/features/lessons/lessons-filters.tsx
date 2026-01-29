import * as React from "react";
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
  setFilterCategoryIds,
  useGetLessonFilter,
} from "./model/lessons-filters-store";
import { MultiSelect, Option } from "@/shared/components/ui/multi-select";

export function LessonsFilters() {
  const { search, isDeleted, categoryIds } = useGetLessonFilter();

  const [localSearch, setLocalSearch] = useState<string>(search ?? "");
  const [debouncedSearch] = useDebounce(localSearch, 300);

  // Фиктивные данные для категорий
  const [categories, setCategories] = useState<Option[]>([
    { value: "math", label: "Математика" },
    { value: "science", label: "Наука" },
    { value: "history", label: "История" },
    { value: "literature", label: "Литература" },
    { value: "programming", label: "Программирование" },
    { value: "design", label: "Дизайн" },
  ]);

  // Состояния для бесконечной прокрутки
  const [isLoading, setIsLoading] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const pageRef = React.useRef(1);

  useEffect(() => {
    setFilterSearch(debouncedSearch);
  }, [debouncedSearch]);

  // Функция для загрузки дополнительных данных
  const loadMore = React.useCallback(async () => {
    setIsLoading(true);

    // Имитация асинхронной загрузки данных
    await new Promise((resolve) => setTimeout(resolve, 1000));

    const currentPage = pageRef.current;
    const newCategories: Option[] = [
      { value: `math-${currentPage}`, label: `Математика ${currentPage}` },
      { value: `science-${currentPage}`, label: `Наука ${currentPage}` },
      { value: `history-${currentPage}`, label: `История ${currentPage}` },
      {
        value: `literature-${currentPage}`,
        label: `Литература ${currentPage}`,
      },
      {
        value: `programming-${currentPage}`,
        label: `Программирование ${currentPage}`,
      },
      { value: `design-${currentPage}`, label: `Дизайн ${currentPage}` },
    ];

    setCategories((prev) => [...prev, ...newCategories]);
    pageRef.current += 1;

    // Остановим загрузку после 3 страниц для демонстрации
    if (currentPage >= 3) {
      setHasMore(false);
    }

    setIsLoading(false);
  }, []);

  return (
    <div className="flex flex-col gap-4 mb-4">
      <div className="flex gap-4">
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
      <MultiSelect
        options={categories}
        selectedValues={categoryIds}
        onValueChange={setFilterCategoryIds}
        placeholder="Выберите категории..."
        isLoading={isLoading}
        loadMore={loadMore}
        hasMore={hasMore}
      />
    </div>
  );
}

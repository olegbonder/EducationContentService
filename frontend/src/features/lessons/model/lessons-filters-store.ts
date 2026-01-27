import { create } from "zustand";
import { PAGE_SIZE } from "./use-lessons-list";
import { useShallow } from "zustand/react/shallow";

export type LessonsFilterState = {
  search?: string;
  isDeleted?: boolean;
  categoryIds?: string[]; // Новый фильтр для категорий
  pageSize: number;
};

type Actions = {
  setSearch: (input: LessonsFilterState["search"]) => void;
  setIsDeleted: (isDeleted: LessonsFilterState["isDeleted"]) => void;
  setCategoryIds: (categoryIds: LessonsFilterState["categoryIds"]) => void;
};

type LessonsFilterStore = LessonsFilterState & Actions;

const initialState: LessonsFilterState = {
  search: "",
  isDeleted: undefined,
  categoryIds: [], // Инициализируем пустым массивом
  pageSize: PAGE_SIZE,
};

const useLessonsFilterStore = create<LessonsFilterStore>((set) => ({
  ...initialState,
  setSearch: (input: LessonsFilterState["search"]) =>
    set(() => ({ search: input?.trim() || undefined })),
  setIsDeleted: (isDeleted: boolean | undefined) => set(() => ({ isDeleted })),
  setCategoryIds: (categoryIds: LessonsFilterState["categoryIds"]) =>
    set(() => ({ categoryIds: categoryIds || [] })),
}));

export const useGetLessonFilter = () => {
  return useLessonsFilterStore(
    useShallow((state) => ({
      search: state.search,
      isDeleted: state.isDeleted,
      categoryIds: state.categoryIds,
      pageSize: state.pageSize,
    }))
  );
};

export const setFilterSearch = (input: LessonsFilterState["search"]) => {
  return useLessonsFilterStore.getState().setSearch(input);
};

export const setFilterIsDeleted = (input: LessonsFilterState["isDeleted"]) => {
  return useLessonsFilterStore.getState().setIsDeleted(input);
};

export const setFilterCategoryIds = (input: LessonsFilterState["categoryIds"]) => {
  return useLessonsFilterStore.getState().setCategoryIds(input);
};

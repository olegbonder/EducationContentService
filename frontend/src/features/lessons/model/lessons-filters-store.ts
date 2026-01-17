import { create } from "zustand";
import { PAGE_SIZE } from "./use-lessons-list";
import { useShallow } from "zustand/react/shallow";
import { createJSONStorage, persist } from "zustand/middleware";

export type LessonsFilterState = {
  search?: string;
  isDeleted?: boolean;
  pageSize: number;
};

type Actions = {
  setSearch: (input: LessonsFilterState["search"]) => void;
  setIsDeleted: (isDeleted: LessonsFilterState["isDeleted"]) => void;
};

type LessonsFilterStore = LessonsFilterState & Actions;

const initialState: LessonsFilterState = {
  search: "",
  isDeleted: undefined,
  pageSize: PAGE_SIZE,
};

const useLessonsFilterStore = create<LessonsFilterStore>()(
  persist(
    (set) => ({
      ...initialState,
      setSearch: (input: LessonsFilterState["search"]) =>
        set(() => ({ search: input?.trim() || undefined })),
      setIsDeleted: (isDeleted: boolean | undefined) =>
        set(() => ({ isDeleted })),
    }),
    {
      name: "es-lessons-filters",
      storage: createJSONStorage(() => localStorage),
    },
  ),
);

export const useGetLessonFilter = () => {
  return useLessonsFilterStore(
    useShallow((state) => ({
      search: state.search,
      isDeleted: state.isDeleted,
      pageSize: state.pageSize,
    })),
  );
};

export const setFilterSearch = (input: LessonsFilterState["search"]) => {
  return useLessonsFilterStore.getState().setSearch(input);
};

export const setFilterIsDeleted = (input: LessonsFilterState["isDeleted"]) => {
  return useLessonsFilterStore.getState().setIsDeleted(input);
};

import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

export type GlobalSearchState = {
  globalSearch?: string;
};

type Actions = {
  setSearch: (input: GlobalSearchState["globalSearch"]) => void;
};

type GlobalSearchStore = GlobalSearchState & Actions;

const initialState: GlobalSearchState = {
  globalSearch: "",
};

const useGlobalSearchStore = create<GlobalSearchStore>()(
  persist(
    (set) => ({
      ...initialState,
      setSearch: (input: GlobalSearchState["globalSearch"]) =>
        set(() => ({ globalSearch: input?.trim() || undefined })),
    }),
    { name: "global-search", storage: createJSONStorage(() => localStorage) }
  )
);

export const useGetGlobalSearch = () => {
  return useGlobalSearchStore((state) => state.globalSearch);
};

export const setGlobalSearch = (input: GlobalSearchState["globalSearch"]) =>
  useGlobalSearchStore.getState().setSearch(input);

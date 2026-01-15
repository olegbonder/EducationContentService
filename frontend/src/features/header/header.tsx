import {
  Avatar,
  AvatarFallback,
  AvatarImage,
} from "@/shared/components/ui/avatar";
import { Input } from "@/shared/components/ui/input";
import { routes } from "@/shared/routes";
import {
  setGlobalSearch,
  useGetGlobalSearch,
} from "@/shared/stores/global-search-store";
import Link from "next/link";
import { SidebarTrigger } from "../../shared/components/ui/sidebar";

export default function Header() {
  const globalSearch = useGetGlobalSearch();

  return (
    <header className="sticky top-0 z-50 w-full bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60">
      <div className="flex h-16 items-center justify-between px-4 gap-4">
        <div className="flex items-center gap-2">
          <SidebarTrigger className="md:hidden mb-4" />
          <div className="flex items-center gap-2 font-semibold text-xl">
            <div className="h-8 w-8 bg-red-600 rounded-full flex items-center justify-center text-white font-bold">
              FS
            </div>
            <Link href={routes.home}>
              <span>Fullstack</span>
            </Link>
          </div>
        </div>

        {/* Центр - Поиск */}
        <div className="flex-1 max-w-md mx-auto">
          <Input
            type="search"
            placeholder="Поиск..."
            className="w-80"
            value={globalSearch}
            onChange={(e) => setGlobalSearch(e.target.value)}
          />
        </div>

        {/* Правая часть - Аватарка */}
        <div className="flex items-center gap-4">
          <Avatar className="h-9 w-9 cursor-pointer hover:opacity-80 transition-opacity">
            <AvatarImage src="https://github.com/shadcn.png" alt="User" />
            <AvatarFallback>ST</AvatarFallback>
          </Avatar>
        </div>
      </div>
    </header>
  );
}

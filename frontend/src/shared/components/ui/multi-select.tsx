"use client";

import * as React from "react";
import { CheckIcon, ChevronDownIcon, XIcon } from "lucide-react";
import { Command as CommandPrimitive, useCommandState } from "cmdk";
import { cn } from "@/shared/lib/utils";
import { Badge } from "@/shared/components/ui/badge";
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from "@/shared/components/ui/command";

type Option = {
  value: string;
  label: string;
};

type MultiSelectProps = {
  options: Option[];
  selectedValues?: string[];
  placeholder?: string;
  onValueChange?: (values: string[]) => void;
  isLoading?: boolean;
  loadMore?: () => void;
  hasMore?: boolean;
  className?: string;
};

function MultiSelect({
  options,
  selectedValues = [],
  placeholder = "Выберите опции...",
  onValueChange,
  isLoading = false,
  loadMore,
  hasMore = false,
  className,
}: MultiSelectProps) {
  const inputRef = React.useRef<HTMLInputElement>(null);
  const containerRef = React.useRef<HTMLDivElement>(null);
  const [open, setOpen] = React.useState(false);
  const [inputValue, setInputValue] = React.useState("");
  const [isLoadingMore, setIsLoadingMore] = React.useState(false);

  // Закрытие списка при клике вне компонента
  React.useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setOpen(false);
      }
    }

    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  const handleUnselect = React.useCallback(
    (option: string) => {
      const newSelectedValues = selectedValues.filter((s) => s !== option);
      onValueChange?.(newSelectedValues);
    },
    [selectedValues, onValueChange]
  );

  const handleKeyDown = React.useCallback(
    (event: React.KeyboardEvent<HTMLDivElement>) => {
      const input = inputRef.current;
      if (!input) return;

      if (event.key === "Delete" || event.key === "Backspace") {
        if (input.value === "" && selectedValues.length > 0) {
          handleUnselect(selectedValues[selectedValues.length - 1]);
        }
      }

      if (event.key === "Escape") {
        input.blur();
      }
    },
    [handleUnselect, selectedValues]
  );

  const handleLoadMore = React.useCallback(() => {
    if (hasMore && loadMore && !isLoadingMore) {
      setIsLoadingMore(true);
      Promise.resolve(loadMore()).finally(() => {
        setIsLoadingMore(false);
      });
    }
  }, [hasMore, loadMore, isLoadingMore]);

  // Обработка скролла для бесконечной загрузки
  const handleScroll = React.useCallback(
    (event: React.UIEvent<HTMLDivElement>) => {
      const { scrollTop, scrollHeight, clientHeight } = event.currentTarget;
      const threshold = 10;
      const atBottom = scrollHeight - scrollTop <= clientHeight + threshold;

      if (atBottom && hasMore) {
        handleLoadMore();
      }
    },
    [handleLoadMore, hasMore]
  );

  return (
    <div ref={containerRef}>
      <Command
        onKeyDown={handleKeyDown}
        className="overflow-visible bg-transparent"
      >
        <div
          className={cn(
            "group border border-input rounded-md px-3 py-2 text-sm ring-offset-background focus-within:ring-2 focus-within:ring-ring focus-within:ring-offset-2",
            className
          )}
        >
        <div className="flex flex-wrap gap-1">
          {selectedValues.map((value) => {
            const option = options.find((o) => o.value === value);
            return (
              <Badge
                key={value}
                variant="secondary"
                className="rounded-sm px-1.5 font-normal"
              >
                {option?.label}
                <button
                  className="ml-1 rounded-full outline-none ring-offset-background focus:ring-2 focus:ring-ring focus:ring-offset-2"
                  onKeyDown={(e) => {
                    if (e.key === "Enter") {
                      handleUnselect(value);
                    }
                  }}
                  onMouseDown={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                  }}
                  onClick={() => handleUnselect(value)}
                >
                  <XIcon className="h-3 w-3 text-muted-foreground hover:text-foreground" />
                </button>
              </Badge>
            );
          })}
          <CommandPrimitive.Input
            ref={inputRef}
            value={inputValue}
            onValueChange={setInputValue}
            onBlur={() => {}}
            onFocus={() => setOpen(true)}
            placeholder={placeholder}
            className="ml-2 flex-1 bg-transparent outline-none placeholder:text-muted-foreground"
          />
          <ChevronDownIcon
            className={`h-4 w-4 shrink-0 text-muted-foreground transition-transform duration-200 ${
              open ? "rotate-180" : ""
            }`}
          />
        </div>
      </div>
      <div className="relative mt-2">
        {open && (
          <CommandList
            onScroll={handleScroll}
            className="absolute top-0 z-10 w-full rounded-md border bg-popover text-popover-foreground shadow-md outline-none animate-in"
          >
            {isLoading ? (
              <CommandEmpty>Загрузка...</CommandEmpty>
            ) : (
              <>
                <CommandGroup className="max-h-64 overflow-auto">
                  {options.map((option) => {
                    const isSelected = selectedValues.includes(option.value);
                    return (
                      <CommandItem
                        key={option.value}
                        onSelect={() => {
                          if (isSelected) {
                            handleUnselect(option.value);
                          } else {
                            onValueChange?.([...selectedValues, option.value]);
                          }
                        }}
                        className="cursor-pointer"
                      >
                        <div
                          className={cn(
                            "mr-2 flex h-4 w-4 items-center justify-center rounded-sm border border-primary",
                            isSelected
                              ? "bg-primary text-primary-foreground"
                              : "opacity-50 [&_svg]:invisible"
                          )}
                        >
                          <CheckIcon className="h-4 w-4" />
                        </div>
                        <span>{option.label}</span>
                      </CommandItem>
                    );
                  })}
                </CommandGroup>
                {isLoadingMore && (
                  <div className="py-2 text-center text-sm">Загрузка...</div>
                )}
              </>
            )}
          </CommandList>
        )}
      </div>
    </Command>
    </div>
  );
}

export { MultiSelect };
export type { MultiSelectProps, Option };
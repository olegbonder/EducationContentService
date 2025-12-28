"use client";

import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import useCounter from "@/shared/hooks/use-counter";

export default function Counter() {
  const { counter, click, isWin } = useCounter();

  return (
    <div className="flex flex-col gap-5">
      <CoolCount count={counter} />
      <Button onClick={click} className="w-fit">
        Увеличить
      </Button>

      <Input
        type="text"
        placeholder="Max letter"
        className="max-w-sm rounded-lg border-2 border-border bg-card px-4 py-2 text-foreground"
      />

      {isWin && <span>Поздравляю!</span>}
    </div>
  );
}

type Props = {
  count: number;
};

function CoolCount({ count }: Props) {
  return <span className="text-red-500">{count}</span>;
}

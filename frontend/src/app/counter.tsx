"use client";

import { Button } from "@/components/ui/button";

export default function Counter() {
    const count = calulateSum(5, 10);

    function calulateSum(a: number, b: number): number {
        return a + b;    
    }

    const handleClick = () => {
        console.log(count);
    }

    return <div className="flex flex-col gap-5">
        <CoolCount count={count} />
        <Button
            onClick={handleClick}
            className="w-fit"
        >Увеличить</Button>
    </div>    
}

type Props = {
    count: number;
}

function CoolCount({count}:Props) {
    return <span className="text-red-500">{count}</span>
}
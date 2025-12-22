"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useEffect, useState } from "react";

export default function Counter() {
    const [counter, setCounter] = useState(0);

    useEffect(() => {
        console.log("Counter mounted")
    }, [counter]);    

    const handleClick = () => {
        setCounter(counter + 1)        
    }

    const isWin = counter >= 10;

    return <div className="flex flex-col gap-5">
        <CoolCount count={counter} />
        <Button
            onClick={handleClick}
            className="w-fit"
        >Увеличить</Button>

        <Input 
            type="text" 
            placeholder="Max letter"
            className="max-w-sm rounded-lg border-2 border-border bg-card px-4 py-2 text-foreground"/>

        {isWin && <span>Поздравляю!</span>}         
    </div>    
}

type Props = {
    count: number;
}

function CoolCount({count}:Props) {
    return <span className="text-red-500">{count}</span>
}
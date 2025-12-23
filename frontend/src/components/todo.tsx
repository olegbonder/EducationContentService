import { Button } from "./ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { Input } from "./ui/input";
import { useState } from "react";
import { Checkbox } from "./ui/checkbox";
import { Badge } from "./ui/badge";

type Todo = {
  id: number;
  text: string;
  completed: boolean;
};

export default function Todo() {
  const [todos, setTodos] = useState<Todo[]>([
    { id: 1, text: "Learn TypeScript", completed: false },
    { id: 2, text: "Build a Next.js app", completed: true },
    { id: 3, text: "Write tests", completed: false },
  ]);

  const [input, setInput] = useState("");

  const addTodo = () => {
    const newTodo: Todo = {
      id: todos.length + 1,
      text: input,
      completed: false,
    };

    setTodos((prevTodos) => [...prevTodos, newTodo]);
  };

  const toggleTodo = (id: number) => {
    setTodos((prevTodos) =>
      prevTodos.map((todo) => {
        if (todo.id === id) {
          return { ...todo, completed: !todo.completed };
        }

        return todo;
      })
    );
  };

  return (
    <div className="space-y-6">
      <div className="flex gap-2">
        <Input
          className="flex gap-2"
          placeholder="Название задачи"
          value={input}
          onChange={(event) => setInput(event.target.value)}
        />
        <Button onClick={() => addTodo()}>Добавить</Button>
      </div>
      <div className="space-y-3">
        {[...todos].reverse().map((todo) => (
          <Card key={todo.id}>
            <CardHeader>
              <div className="flex items-center justify-between">
                <Checkbox
                  checked={todo.completed}
                  onClick={() => toggleTodo(todo.id)}
                />
                <CardTitle className="text-lg">{todo.text}</CardTitle>
                <Badge variant={todo.completed ? "default" : "destructive"}>
                  {todo.completed ? "Выполнено" : "В процессе"}
                </Badge>
              </div>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground">Задача #{todo.id}</p>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}

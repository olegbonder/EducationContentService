"use client";

import {
  TreeDataItem,
  TreeView,
  TreeRenderItemParams,
} from "@/shared/components/ui/tree-view";
import { Folder, FolderOpen, File, Check, X } from "lucide-react";
import { Skeleton } from "@/shared/components/ui/skeleton";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/shared/components/ui/tooltip";
import React from "react";

export default function Tree() {
  const [loadingNodes, setLoadingNodes] = React.useState<Set<string>>(
    new Set(),
  );
  const [expandedNodes, setExpandedNodes] = React.useState<Set<string>>(
    new Set(),
  );

  interface Department extends TreeDataItem {
    isActive: boolean;
    path: string;
    hasMore: boolean;
  }
  const data: Department[] = [
    {
      id: "1",
      name: "Item 1",
      path: "root",
      isActive: true,
      hasMore: true,
      children: [
        {
          id: "2",
          name: "Item 1.1",
          path: "root.item11",
          isActive: false,
          children: [
            {
              id: "3",
              name: "Item 1.1.1",
              path: "root.item11.item111",
              isActive: false,
            },
            {
              id: "4",
              name: "Item 1.1.2",
              path: "root.item11.item112",
              isActive: false,
            },
          ],
        },
        {
          id: "5",
          name: "Item 1.2 (disabled)",
          path: "root.item12",
          isActive: false,
          disabled: true,
        },
      ],
    },
    {
      id: "6",
      name: "Item 2 (draggable)",
      path: "root2",
      isActive: false,
      draggable: true,
    },
  ];

  // Функция для обработки раскрытия/сворачивания узла
  const handleNodeToggle = (itemId: string) => {
    setExpandedNodes((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(itemId)) {
        // Сворачивание - удаляем из раскрытых, но не показываем загрузку
        newSet.delete(itemId);
        return newSet;
      } else {
        // Раскрытие - показываем состояние загрузки
        setLoadingNodes((loadPrev) => new Set(loadPrev).add(itemId));

        // Симулируем задержку при загрузке
        setTimeout(() => {
          setLoadingNodes((loadPrev) => {
            const updatedSet = new Set(loadPrev);
            updatedSet.delete(itemId);
            return updatedSet;
          });

          // После "загрузки" добавляем узел в раскрытые
          setExpandedNodes((expPrev) => new Set(expPrev).add(itemId));
        }, 1000);

        return prev; // Возвращаем старое состояние до завершения асинхронной операции
      }
    });
  };

  const renderItem = ({
    item,
    level,
    isLeaf,
    isSelected,
    isOpen,
    hasChildren,
  }: TreeRenderItemParams) => {
    let IconComponent;
    const isLoading = loadingNodes.has(item.id);
    const isActuallyExpanded = expandedNodes.has(item.id);

    if (hasChildren) {
      if (isActuallyExpanded && !isLoading) {
        IconComponent = FolderOpen;
      } else {
        IconComponent = Folder;
      }
    } else {
      // Для листьев используем иконку файла
      IconComponent = File;
    }

    // Функция для получения пути к узлу
    const getNodePath = (
      currentItem: TreeDataItem,
      allData: TreeDataItem[],
    ): string => {
      // Создаем плоский массив всех узлов с информацией о родителях
      const findPath = (
        nodes: TreeDataItem[],
        currentPath: string = "",
      ): string | null => {
        for (const node of nodes) {
          const newPath = currentPath
            ? `${currentPath} / ${node.name}`
            : node.name;

          if (node.id === currentItem.id) {
            return newPath;
          }

          if (node.children) {
            const foundPath = findPath(node.children, newPath);
            if (foundPath) {
              return foundPath;
            }
          }
        }
        return null;
      };

      return findPath(allData) || currentItem.name;
    };

    // Приведение типа для доступа к дополнительному свойству isActive
    const itemWithIsActive = item as TreeDataItem & { isActive?: boolean };

    const itemPath = getNodePath(item, data);

    const content = (
      <TooltipProvider>
        <Tooltip>
          <TooltipTrigger asChild>
            <div className="flex items-center justify-between w-full">
              <div className="flex items-center">
                <IconComponent className="h-4 w-4 shrink-0 mr-2" />
                <span className="text-sm truncate">{item.name}</span>
              </div>
              {typeof itemWithIsActive.isActive !== "undefined" && (
                <div className="ml-2 flex items-center">
                  {itemWithIsActive.isActive ? (
                    <Check className="w-4 h-4 text-green-500" />
                  ) : (
                    <X className="w-4 h-4 text-red-500" />
                  )}
                </div>
              )}
            </div>
          </TooltipTrigger>
          <TooltipContent>
            <p>{itemPath}</p>
          </TooltipContent>
        </Tooltip>
      </TooltipProvider>
    );

    return (
      <div>
        {content}
        {/* Показываем skeleton-элементы как дочерние при загрузке */}
        {isLoading && (
          <div className="ml-4 pl-1 border-l mt-1 space-y-1">
            {[...Array(3)].map((_, index) => (
              <div key={index} className="flex items-center py-1">
                <Skeleton className="h-4 w-4 shrink-0 mr-2 rounded" />
                <Skeleton className="h-4 w-16 rounded" />
              </div>
            ))}
          </div>
        )}
      </div>
    );
  };

  return (
    <TooltipProvider>
      <div>
        <TreeView
          data={data}
          renderItem={renderItem}
          onSelectChange={(item) => {
            if (item && item.children && item.children.length > 0) {
              handleNodeToggle(item.id);
            }
          }}
        />
      </div>
    </TooltipProvider>
  );
}

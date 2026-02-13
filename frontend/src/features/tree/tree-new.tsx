import {
  TreeProvider,
  TreeView,
  TreeNode,
  TreeNodeTrigger,
  TreeNodeContent,
  TreeExpander,
  TreeIcon,
  TreeLabel,
} from "@/shared/components/ui/tree";
import { Folder, FolderOpen, File, Circle, Plus } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { useState } from "react";

interface Department {
  id: string;
  name: string;
  path: string;
  isActive: boolean;
  hasMore?: boolean;
  children?: Department[];
  disabled?: boolean;
  draggable?: boolean;
}

// Рекурсивная функция для рендеринга узлов дерева
const renderTreeNodes = (
  nodes: Department[],
  level: number = 0,
  visibleChildrenCounts: Record<string, number>,
  onShowMoreChildren: (id: string) => void,
) => {
  return nodes.map((node) => {
    // Определяем количество видимых детей для этого узла
    const totalChildren = node.children ? node.children.length : 0;
    const visibleCount =
      visibleChildrenCounts[node.id] || (level === 0 ? 3 : 2); // Для корневых узлов показываем 3, для остальных - 4
    const visibleChildren = node.children
      ? node.children.slice(0, visibleCount)
      : [];

    return (
      <TreeNode key={node.id} nodeId={node.id} level={level}>
        <TreeNodeTrigger>
          <TreeExpander
            hasChildren={!!node.children && node.children.length > 0}
          />
          <TreeIcon
            hasChildren={!!node.children && node.children.length > 0}
            icon={
              node.children && node.children.length > 0 ? (
                node.isActive ? (
                  <FolderOpen className="h-4 w-4" />
                ) : (
                  <Folder className="h-4 w-4" />
                )
              ) : (
                <File className="h-4 w-4" />
              )
            }
          />
          <TreeLabel className={!node.isActive ? "text-gray-500" : ""}>
            {node.name}
          </TreeLabel>
        </TreeNodeTrigger>
        <TreeNodeContent
          hasChildren={!!node.children && node.children.length > 0}
        >
          {visibleChildren &&
            visibleChildren.length > 0 &&
            renderTreeNodes(
              visibleChildren,
              level + 1,
              visibleChildrenCounts,
              onShowMoreChildren,
            )}
          {totalChildren > visibleCount && (
            <div className="mt-1" style={{ paddingLeft: (level + 1) * 20 + 8 }}>
              <Button
                variant="ghost"
                size="sm"
                className="text-xs h-6"
                onClick={(e) => {
                  e.stopPropagation(); // Предотвращаем всплытие события
                  onShowMoreChildren(node.id);
                }}
              >
                <Plus className="h-3 w-3 mr-1" />
                Показать ещё
              </Button>
            </div>
          )}
        </TreeNodeContent>
      </TreeNode>
    );
  });
};

export default function TreeNew() {
  const [visibleChildrenCounts, setVisibleChildrenCounts] = useState<
    Record<string, number>
  >({});

  const data: Department[] = [
    {
      id: "1",
      name: "Компания ООО Рога и Копыта",
      path: "root",
      isActive: true,
      hasMore: true,
      children: [
        {
          id: "2",
          name: "Отдел разработки",
          path: "root.dev",
          isActive: true,
          children: [
            {
              id: "3",
              name: "Фронтенд команда",
              path: "root.dev.frontend",
              isActive: true,
              children: [
                {
                  id: "7",
                  name: "React разработчики",
                  path: "root.dev.frontend.react",
                  isActive: false,
                },
                {
                  id: "8",
                  name: "Vue разработчики",
                  path: "root.dev.frontend.vue",
                  isActive: false,
                },
                {
                  id: "17",
                  name: "Angular разработчики",
                  path: "root.dev.frontend.angular",
                  isActive: false,
                },
                {
                  id: "18",
                  name: "Svelte разработчики",
                  path: "root.dev.frontend.svelte",
                  isActive: false,
                },
              ],
            },
            {
              id: "4",
              name: "Бэкенд команда",
              path: "root.dev.backend",
              isActive: false,
              children: [
                {
                  id: "9",
                  name: "Node.js разработчики",
                  path: "root.dev.backend.node",
                  isActive: false,
                },
                {
                  id: "10",
                  name: "Python разработчики",
                  path: "root.dev.backend.python",
                  isActive: false,
                },
                {
                  id: "19",
                  name: "Java разработчики",
                  path: "root.dev.backend.java",
                  isActive: false,
                },
              ],
            },
            {
              id: "20",
              name: "DevOps команда",
              path: "root.dev.devops",
              isActive: false,
              children: [
                {
                  id: "21",
                  name: "Специалисты по Docker",
                  path: "root.dev.devops.docker",
                  isActive: false,
                },
                {
                  id: "22",
                  name: "Специалисты по Kubernetes",
                  path: "root.dev.devops.k8s",
                  isActive: false,
                },
              ],
            },
          ],
        },
        {
          id: "5",
          name: "Отдел маркетинга",
          path: "root.marketing",
          isActive: false,
          children: [
            {
              id: "11",
              name: "Контент-менеджеры",
              path: "root.marketing.content",
              isActive: false,
            },
            {
              id: "12",
              name: "Таргетологи",
              path: "root.marketing.target",
              isActive: false,
            },
            {
              id: "23",
              name: "SEO специалисты",
              path: "root.marketing.seo",
              isActive: false,
            },
            {
              id: "24",
              name: "SMM специалисты",
              path: "root.marketing.smm",
              isActive: false,
            },
          ],
        },
        {
          id: "6",
          name: "Отдел продаж",
          path: "root.sales",
          isActive: false,
          children: [
            {
              id: "13",
              name: "B2B отдел",
              path: "root.sales.b2b",
              isActive: false,
            },
            {
              id: "14",
              name: "B2C отдел",
              path: "root.sales.b2c",
              isActive: false,
            },
            {
              id: "25",
              name: "Онлайн-продажи",
              path: "root.sales.online",
              isActive: false,
            },
          ],
        },
        {
          id: "15",
          name: "HR отдел (отключен)",
          path: "root.hr",
          isActive: false,
          disabled: true,
        },
        {
          id: "16",
          name: "Финансовый отдел (перемещаемый)",
          path: "root.finance",
          isActive: false,
          draggable: true,
        },
        {
          id: "26",
          name: "Отдел безопасности",
          path: "root.security",
          isActive: false,
        },
        {
          id: "27",
          name: "Юридический отдел",
          path: "root.legal",
          isActive: false,
        },
      ],
    },
  ];

  const onShowMoreChildren = (nodeId: string) => {
    setVisibleChildrenCounts((prev) => {
      const node = findNodeById(data, nodeId);
      if (!node || !node.children) return prev;

      // Определяем уровень узла
      const level = getLevelOfNode(data, nodeId);
      const increment = level === 0 ? 3 : 4; // Для корневых - 3, для остальных - 4
      const currentCount = prev[nodeId] || (level === 0 ? 3 : 4);
      const totalCount = node.children.length;

      // Увеличиваем количество видимых детей, но не больше общего количества
      const newCount = Math.min(currentCount + increment, totalCount);
      return { ...prev, [nodeId]: newCount };
    });
  };

  return (
    <div className="p-4">
      <h2 className="text-xl font-bold mb-4">Структура компании</h2>

      <TreeProvider>
        <TreeView>
          {renderTreeNodes(data, 0, visibleChildrenCounts, onShowMoreChildren)}
        </TreeView>
      </TreeProvider>
    </div>
  );
}

// Вспомогательная функция для поиска узла по ID
function findNodeById(nodes: Department[], id: string): Department | undefined {
  for (const node of nodes) {
    if (node.id === id) {
      return node;
    }
    if (node.children) {
      const found = findNodeById(node.children, id);
      if (found) return found;
    }
  }
  return undefined;
}

// Вспомогательная функция для определения уровня узла
function getLevelOfNode(
  nodes: Department[],
  targetId: string,
  currentLevel: number = 0,
): number | undefined {
  for (const node of nodes) {
    if (node.id === targetId) {
      return currentLevel;
    }
    if (node.children) {
      const level = getLevelOfNode(node.children, targetId, currentLevel + 1);
      if (level !== undefined) {
        return level;
      }
    }
  }
  return undefined;
}

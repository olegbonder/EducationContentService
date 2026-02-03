import React, { useState } from 'react';
import { TreeView, TreeDataItem } from "@/shared/components/ui/tree-view";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { Folder, File, FolderOpen, CircleX, CircleCheck } from 'lucide-react';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/shared/components/ui/tooltip';
import { useLazyTree } from '@/entities/department/hooks/useLazyTree';
import { DepartmentWithChildren } from '@/entities/department/api/department-api';

// Enhanced tree item with all the requested features
interface EnhancedTreeItem extends TreeDataItem {
  depth?: number;
  path?: string;
  hasMoreChildren?: boolean;
  isActive?: boolean;
  isLoading?: boolean;
  loadMoreHandler?: () => void;
  showLoadMore?: boolean;
}

const EnhancedTreeItem: React.FC<{
  item: EnhancedTreeItem;
  level: number;
  isOpen?: boolean;
  hasChildren?: boolean;
}> = ({ item, level, isOpen, hasChildren }) => {
  const paddingClass = `pl-${Math.max(4, level * 4)}`;

  // Choose icon based on state (open/closed) and whether item has children
  let IconComponent = File; // Default to File icon

  if (hasChildren) {
    if (isOpen) {
      IconComponent = FolderOpen; // Use open folder when expanded
    } else {
      IconComponent = Folder; // Use closed folder when collapsed
    }
  }

  return (
    <div className={`flex items-center py-2 ${paddingClass}`}>
      {/* Icon based on whether item has children and its open/closed state */}
      <IconComponent className={`h-4 w-4 shrink-0 mr-2 ${item.isActive === false ? 'text-gray-400' : ''}`} />

      {/* Name with tooltip for path */}
      <TooltipProvider>
        <Tooltip>
          <TooltipTrigger asChild>
            <span
              className={`text-sm truncate ${item.isActive === false ? 'text-gray-400 line-through' : ''}`}
            >
              {item.name}
            </span>
          </TooltipTrigger>
          {item.path && (
            <TooltipContent>
              <p>{item.path}</p>
            </TooltipContent>
          )}
        </Tooltip>
      </TooltipProvider>

      {/* Status indicator for active/inactive */}
      {item.isActive === false && <CircleX className="h-3 w-3 ml-1 text-red-500" />}
      {item.isActive === true && <CircleCheck className="h-3 w-3 ml-1 text-green-500" />}
    </div>
  );
};

const LoadingSkeleton: React.FC<{ level: number }> = ({ level }) => {
  const paddingClass = `pl-${Math.max(8, (level + 1) * 4)}`; // Increase indentation to match children level
  return (
    <div className={`py-1 ${paddingClass}`}>
      <Skeleton className="h-4 w-32" />
    </div>
  );
};

export default function Tree() {
  const {
    initializeTree,
    loadChildren,
    loadMoreChildren,
    hasMoreChildren,
    isLoading,
    getRootNodes,
    treeState
  } = useLazyTree();

  const [initialized, setInitialized] = useState(false);

  // Initialize the tree on mount
  React.useEffect(() => {
    if (!initialized) {
      initializeTree();
      setInitialized(true);
    }
  }, [initializeTree, initialized]);

  // Convert department data to tree-compatible format
  const convertToTreeData = (departments: DepartmentWithChildren[]): EnhancedTreeItem[] => {
    return departments.map(dep => {
      const hasChildren = dep.children && dep.children.length > 0;
      const hasMore = hasChildren && hasMoreChildren(dep.id);
      const isCurrentlyLoading = isLoading(dep.id);

      const treeItem: EnhancedTreeItem = {
        id: dep.id,
        name: dep.name,
        depth: dep.depth,
        path: dep.path,
        isActive: dep.isActive,
        hasMoreChildren: dep.hasMoreChildren,
        isLoading: isCurrentlyLoading,

        // Custom icon logic
        icon: dep.children && dep.children.length > 0 ? Folder : File,
        openIcon: FolderOpen,
        selectedIcon: dep.isActive === false ? CircleX : CircleCheck,

        // Custom rendering
        className: dep.isActive === false ? 'opacity-60 line-through' : '',

        // Click handler to load children if needed
        onClick: dep.hasMoreChildren && (!dep.children || dep.children.length === 0)
          ? () => loadChildren(dep.id)
          : undefined,
      };

      // Add children if they exist
      if (hasChildren) {
        treeItem.children = convertToTreeData(dep.children!);
      }

      // Add load more button if needed
      if (hasMore) {
        treeItem.showLoadMore = true;
        treeItem.loadMoreHandler = () => loadMoreChildren(dep.id);
      }

      return treeItem;
    });
  };

  const rootNodes = getRootNodes();
  const treeData = convertToTreeData(rootNodes);

  // Custom renderer to handle all the requested features
  const customRenderer = (params: {
    item: EnhancedTreeItem;
    level: number;
    isLeaf: boolean;
    isSelected: boolean;
    isOpen?: boolean;
    hasChildren: boolean;
  }) => {
    const { item, level, isLeaf, isOpen, hasChildren } = params;

    return (
      <div className="w-full">
        <EnhancedTreeItem item={item} level={level} isOpen={isOpen} hasChildren={hasChildren} />

      </div>
    );
  };

  return (
    <div className="p-4">
      <TreeView
        data={treeData}
        renderItem={customRenderer}
        defaultNodeIcon={Folder}
        defaultLeafIcon={File}
      />
    </div>
  );
}

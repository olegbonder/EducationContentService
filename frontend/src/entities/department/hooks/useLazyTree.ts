import { useState, useCallback, useEffect } from 'react';
import { DepartmentWithChildren, departmentApi } from '../api/department-api';
import { mockDepartmentApi } from '../api/mock-department-api';

export interface LazyTreeState {
  nodes: Record<string, DepartmentWithChildren>;
  loadingNodes: Set<string>;
  hasMoreMap: Record<string, boolean>;
  currentPageMap: Record<string, number>;
}

export const useLazyTree = () => {
  const [treeState, setTreeState] = useState<LazyTreeState>({
    nodes: {},
    loadingNodes: new Set(),
    hasMoreMap: {},
    currentPageMap: {}
  });

  // Initialize with root departments
  const initializeTree = useCallback(async () => {
    setTreeState(prev => ({
      ...prev,
      loadingNodes: new Set([...prev.loadingNodes, 'root'])
    }));

    try {
      const response = await departmentApi.getRootDepartments().catch(() => {
        // Fallback to mock API if real API fails
        return mockDepartmentApi.getRootDepartments();
      });

      const newNodes: Record<string, DepartmentWithChildren> = {};
      const newHasMoreMap: Record<string, boolean> = { ...treeState.hasMoreMap };
      const newCurrentPageMap: Record<string, number> = { ...treeState.currentPageMap };

      response.items.forEach(department => {
        newNodes[department.id] = department;
        newHasMoreMap[department.id] = department.hasMoreChildren;
        newCurrentPageMap[department.id] = department.childrenPage || 1;
      });

      setTreeState(prev => ({
        nodes: { ...prev.nodes, ...newNodes },
        loadingNodes: new Set([...prev.loadingNodes].filter(id => id !== 'root')),
        hasMoreMap: newHasMoreMap,
        currentPageMap: newCurrentPageMap
      }));
    } catch (error) {
      console.error('Failed to load root departments:', error);
      setTreeState(prev => ({
        ...prev,
        loadingNodes: new Set([...prev.loadingNodes].filter(id => id !== 'root'))
      }));
    }
  }, [treeState.hasMoreMap, treeState.currentPageMap]);

  // Load children for a specific parent node
  const loadChildren = useCallback(async (parentId: string) => {
    if (treeState.loadingNodes.has(parentId)) return;

    setTreeState(prev => ({
      ...prev,
      loadingNodes: new Set([...prev.loadingNodes, parentId])
    }));

    try {
      const currentPage = treeState.currentPageMap[parentId] || 1;
      const response = await departmentApi.getDepartmentChildren(parentId, currentPage).catch(() => {
        // Fallback to mock API if real API fails
        return mockDepartmentApi.getDepartmentChildren(parentId, currentPage);
      });

      setTreeState(prev => {
        const newNodes = { ...prev.nodes };
        const newHasMoreMap = { ...prev.hasMoreMap };
        const newCurrentPageMap = { ...prev.currentPageMap };

        // Add new children to the parent
        const parentNode = newNodes[parentId];
        if (parentNode) {
          const existingChildren = parentNode.children || [];
          const newChildren = response.items;

          // Update parent's children with new batch
          newNodes[parentId] = {
            ...parentNode,
            children: [...existingChildren, ...newChildren],
            totalChildrenCount: response.totalCount
          };
        }

        // Add new child nodes to the global map
        response.items.forEach(child => {
          if (!newNodes[child.id]) {
            newNodes[child.id] = child;
          }
          newHasMoreMap[child.id] = child.hasMoreChildren;
          newCurrentPageMap[child.id] = child.childrenPage || 1;
        });

        // Update hasMore status for parent
        newHasMoreMap[parentId] = response.page < response.totalPages;

        return {
          nodes: newNodes,
          loadingNodes: new Set([...prev.loadingNodes].filter(id => id !== parentId)),
          hasMoreMap: newHasMoreMap,
          currentPageMap: newCurrentPageMap
        };
      });
    } catch (error) {
      console.error(`Failed to load children for department ${parentId}:`, error);
      setTreeState(prev => ({
        ...prev,
        loadingNodes: new Set([...prev.loadingNodes].filter(id => id !== parentId))
      }));
    }
  }, [treeState.loadingNodes, treeState.currentPageMap]);

  // Load more children for a specific parent node
  const loadMoreChildren = useCallback(async (parentId: string) => {
    if (treeState.loadingNodes.has(parentId)) return;

    setTreeState(prev => ({
      ...prev,
      loadingNodes: new Set([...prev.loadingNodes, parentId]),
      currentPageMap: {
        ...prev.currentPageMap,
        [parentId]: (prev.currentPageMap[parentId] || 1) + 1
      }
    }));

    try {
      const nextPage = (treeState.currentPageMap[parentId] || 1) + 1;
      const response = await departmentApi.getDepartmentChildren(parentId, nextPage).catch(() => {
        // Fallback to mock API if real API fails
        return mockDepartmentApi.getDepartmentChildren(parentId, nextPage);
      });

      setTreeState(prev => {
        const newNodes = { ...prev.nodes };
        const newHasMoreMap = { ...prev.hasMoreMap };
        const newCurrentPageMap = { ...prev.currentPageMap };

        // Add new children to the parent
        const parentNode = newNodes[parentId];
        if (parentNode) {
          const existingChildren = parentNode.children || [];
          const newChildren = response.items;

          // Update parent's children with new batch
          newNodes[parentId] = {
            ...parentNode,
            children: [...existingChildren, ...newChildren],
            totalChildrenCount: response.totalCount
          };
        }

        // Add new child nodes to the global map
        response.items.forEach(child => {
          if (!newNodes[child.id]) {
            newNodes[child.id] = child;
          }
          newHasMoreMap[child.id] = child.hasMoreChildren;
          newCurrentPageMap[child.id] = child.childrenPage || 1;
        });

        // Update hasMore status for parent
        newHasMoreMap[parentId] = response.page < response.totalPages;

        return {
          nodes: newNodes,
          loadingNodes: new Set([...prev.loadingNodes].filter(id => id !== parentId)),
          hasMoreMap: newHasMoreMap,
          currentPageMap: newCurrentPageMap
        };
      });
    } catch (error) {
      console.error(`Failed to load more children for department ${parentId}:`, error);
      setTreeState(prev => ({
        ...prev,
        loadingNodes: new Set([...prev.loadingNodes].filter(id => id !== parentId)),
        currentPageMap: {
          ...prev.currentPageMap,
          [parentId]: (prev.currentPageMap[parentId] || 1) - 1 // Revert page number on error
        }
      }));
    }
  }, [treeState.loadingNodes, treeState.currentPageMap]);

  // Check if a node has more children to load
  const hasMoreChildren = useCallback((nodeId: string) => {
    return treeState.hasMoreMap[nodeId] || false;
  }, [treeState.hasMoreMap]);

  // Check if a node is currently loading
  const isLoading = useCallback((nodeId: string) => {
    return treeState.loadingNodes.has(nodeId);
  }, [treeState.loadingNodes]);

  // Get a specific node
  const getNode = useCallback((nodeId: string) => {
    return treeState.nodes[nodeId];
  }, [treeState.nodes]);

  // Get root nodes
  const getRootNodes = useCallback(() => {
    return Object.values(treeState.nodes).filter(node => node.parentId === null);
  }, [treeState.nodes]);

  return {
    treeState,
    initializeTree,
    loadChildren,
    loadMoreChildren,
    hasMoreChildren,
    isLoading,
    getNode,
    getRootNodes
  };
};
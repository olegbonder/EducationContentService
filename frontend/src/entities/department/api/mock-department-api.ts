import {
  Department,
  DepartmentWithChildren,
  PaginationResponse,
} from "@/entities/department/api/department-api";

// Base mock data for demonstration
const baseMockDepartments: Department[] = [
  {
    id: "1",
    name: "Корпоративный отдел",
    parentId: null,
    hasMoreChildren: true,
    depth: 0,
    path: "Корпоративный отдел",
    isActive: true,
  },
  {
    id: "2",
    name: "Отдел разработки",
    parentId: null,
    hasMoreChildren: true,
    depth: 0,
    path: "Отдел разработки",
    isActive: true,
  },
  {
    id: "3",
    name: "Отдел продаж",
    parentId: null,
    hasMoreChildren: true,
    depth: 0,
    path: "Отдел продаж",
    isActive: true,
  },
  {
    id: "4",
    name: "Отдел маркетинга",
    parentId: null,
    hasMoreChildren: false,
    depth: 0,
    path: "Отдел маркетинга",
    isActive: false,
  },
  {
    id: "5",
    name: "Финансовый отдел",
    parentId: "1",
    hasMoreChildren: true,
    depth: 1,
    path: "Корпоративный отдел > Финансовый отдел",
    isActive: true,
  },
  {
    id: "6",
    name: "HR отдел",
    parentId: "1",
    hasMoreChildren: true,
    depth: 1,
    path: "Корпоративный отдел > HR отдел",
    isActive: true,
  },
  {
    id: "7",
    name: "IT поддержка",
    parentId: "2",
    hasMoreChildren: false,
    depth: 1,
    path: "Отдел разработки > IT поддержка",
    isActive: true,
  },
  {
    id: "8",
    name: "Frontend команда",
    parentId: "2",
    hasMoreChildren: true,
    depth: 1,
    path: "Отдел разработки > Frontend команда",
    isActive: true,
  },
  {
    id: "9",
    name: "Backend команда",
    parentId: "2",
    hasMoreChildren: true,
    depth: 1,
    path: "Отдел разработки > Backend команда",
    isActive: true,
  },
  {
    id: "10",
    name: "QA команда",
    parentId: "2",
    hasMoreChildren: false,
    depth: 1,
    path: "Отдел разработки > QA команда",
    isActive: false,
  },
];

// Function to generate dynamic mock departments for any level
const generateMockDepartmentsForParent = (
  parentId: string,
  page: number,
  pageSize: number = 5,
): Department[] => {
  const startIndex = (page - 1) * pageSize;
  const departments: Department[] = [];

  // Find parent department to determine depth and path
  const parentDept = baseMockDepartments.find((d) => d.id === parentId);
  const parentDepth = parentDept ? parentDept.depth : 0;
  const parentPath = parentDept ? parentDept.path : parentId;

  // Generate dynamic departments based on parent ID and page
  for (let i = 0; i < pageSize; i++) {
    const index = startIndex + i;
    const id = `${parentId}-child-${index + 1}`;

    departments.push({
      id,
      name: `Подразделение ${index + 1} (${parentId})`,
      parentId,
      hasMoreChildren: index % 3 === 0, // Every third child has more children
      depth: parentDepth + 1,
      path: `${parentPath} > Подразделение ${index + 1}`,
      isActive: index % 4 !== 0, // Every fourth child is inactive
    });
  }

  return departments;
};

// Function to get all departments (base + dynamically generated)
const getAllDepartments = (
  parentId: string | null,
  page: number = 1,
  pageSize: number = 5,
): Department[] => {
  // Get base departments that match the parent
  const baseChildren = baseMockDepartments.filter(
    (dep) => dep.parentId === parentId,
  );

  if (page === 1) {
    // For first page, return base departments if any exist
    if (baseChildren.length > 0) {
      return baseChildren;
    } else {
      // If no base children exist, generate dynamic ones
      return generateMockDepartmentsForParent(
        parentId || "root",
        page,
        pageSize,
      );
    }
  } else {
    // For subsequent pages, return dynamically generated departments
    return generateMockDepartmentsForParent(parentId || "root", page, pageSize);
  }
};

// Simulate API delay
const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

export const mockDepartmentApi = {
  // Get root departments with first N children
  getRootDepartments: async (
    page = 1,
    pageSize = 5,
  ): Promise<PaginationResponse<DepartmentWithChildren>> => {
    await delay(500); // Simulate network delay

    const startIndex = (page - 1) * pageSize;
    const endIndex = startIndex + pageSize;

    const rootDepartments = baseMockDepartments.filter(
      (dep) => dep.parentId === null,
    );
    const paginatedDepartments = rootDepartments.slice(startIndex, endIndex);

    // Add some children to the first few departments to demonstrate the tree structure
    const departmentsWithChildren = paginatedDepartments.map((dep) => {
      const children = getAllDepartments(dep.id, 1, 3); // Get first page with 3 items

      return {
        ...dep,
        children: children.length > 0 ? children : undefined,
        childrenPage: 1,
        totalChildrenCount:
          baseMockDepartments.filter((child) => child.parentId === dep.id)
            .length + 100, // Assume more items available for pagination
      };
    });

    return {
      items: departmentsWithChildren,
      totalCount: rootDepartments.length,
      page,
      pageSize,
      totalPages: Math.ceil(rootDepartments.length / pageSize),
    };
  },

  // Get children of a department
  getDepartmentChildren: async (
    parentId: string,
    page = 1,
    pageSize = 5,
  ): Promise<PaginationResponse<DepartmentWithChildren>> => {
    await delay(500); // Simulate network delay

    // Get children for the specific page
    const children = getAllDepartments(parentId, page, pageSize);

    // Add nested children to demonstrate deeper levels
    const childrenWithNested = children.map((child) => {
      // Only add children if the child node hasMoreChildren set to true
      let nestedChildren: Department[] = [];
      if (child.hasMoreChildren) {
        nestedChildren = getAllDepartments(child.id, 1, 2); // Get first 2 nested children
      }

      return {
        ...child,
        children: nestedChildren.length > 0 ? nestedChildren : undefined,
        childrenPage: page,
        totalChildrenCount:
          page === 1
            ? baseMockDepartments.filter(
                (nested) => nested.parentId === child.id,
              ).length + 100 // Assume more items available
            : 100, // For subsequent pages, assume many items available
      };
    });

    // Calculate total count based on whether it's first page or subsequent
    const totalCount =
      page === 1
        ? baseMockDepartments.filter((dep) => dep.parentId === parentId)
            .length + 100 // Assume more items available
        : 100; // For subsequent pages, return fixed count to allow pagination

    return {
      items: childrenWithNested,
      totalCount,
      page,
      pageSize,
      totalPages: 10, // Fixed number of pages to allow continuous loading
    };
  },
};

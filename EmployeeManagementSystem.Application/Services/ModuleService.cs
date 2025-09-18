using EmployeeManagementSystem.Application.DTOs;
using EmployeeManagementSystem.Application.Interfaces;
using EmployeeManagementSystem.Domain.Entities;

namespace EmployeeManagementSystem.Application.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ModuleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<ModulesResponseDto>> GetModulesAsync(string userRoleRefCode)
        {
            try
            {
                var userRole = await _unitOfWork.UserRoles.FindFirstAsync(ur => ur.RefCode == userRoleRefCode);
                if (userRole == null)
                {
                    return new ApiResponse<ModulesResponseDto>
                    {
                        Success = false,
                        Message = $"User role with ref code '{userRoleRefCode}' does not exist.",
                        Data = null
                    };
                }

                var userRoleAccesses = (await _unitOfWork.UserRoleAccesses.FindAsync((ura) => ura.UserRoleId == userRole.Id))?.ToList();
                if (userRoleAccesses == null || userRoleAccesses.Count == 0)
                {
                    return new ApiResponse<ModulesResponseDto>
                    {
                        Success = false,
                        Message = "No modules found for the specified user role.",
                        Data = null
                    };
                }

                var moduleAccessIds = userRoleAccesses.Select(ura => ura.ModuleAccessId).ToList();
                var moduleAccesses = (await _unitOfWork.ModuleAccesses.FindAsync(ma => moduleAccessIds.Contains(ma.Id)))?.ToList();
                if (moduleAccesses == null || moduleAccesses.Count == 0)
                {
                    return new ApiResponse<ModulesResponseDto>
                    {
                        Success = false,
                        Message = "No module accesses found for the specified user role.",
                        Data = null
                    };
                }

                var moduleIds = moduleAccesses.Select(ma => ma.ModuleId).ToList();
                var modules = (await _unitOfWork.Modules.FindAsync(m => moduleIds.Contains(m.Id) && m.IsVisible))?.ToList();
                if (modules == null || modules.Count == 0)
                {
                    return new ApiResponse<ModulesResponseDto>
                    {
                        Success = false,
                        Message = "No modules found for the specified user role.",
                        Data = null
                    };
                }

                return new ApiResponse<ModulesResponseDto>
                {
                    Success = true,
                    Message = "Allowed modules retrieved successfully.",
                    Data = new ModulesResponseDto
                    {
                        Modules = BuildModuleTree(modules)
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ModulesResponseDto>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving allowed modules: {ex.Message}",
                    Data = null
                };
            }
        }


        private List<ModuleResponseDto> BuildModuleTree(IEnumerable<Module> modules)
        {
            if (modules == null) return new List<ModuleResponseDto>();

            // Map modules to DTOs and prepare children lists
            var dtoMap = modules.Select(m => new ModuleResponseDto
            {
                Id = m.Id,
                ModuleName = m.ModuleName,
                ParentId = m.ParentId,
                RefCode = m.RefCode,
                IsVisible = m.IsVisible,
                LogoName = m.LogoName,
                RedirectPage = m.RedirectPage,
                SortOrder = m.SortOrder,
                Description = m.Description,
                DateCreatedUTC = m.DateCreatedUTC,
                CreatedBy = m.CreatedBy,
                LatestDateUpdatedUTC = m.LatestDateUpdatedUTC,
                LatestUpdatedBy = m.LatestUpdatedBy,
                SubModules = new List<ModuleResponseDto>()
            }).ToDictionary(d => d.Id);

            var roots = new List<ModuleResponseDto>();

            // Attach each node to its parent (if any), otherwise treat as root
            foreach (var node in dtoMap.Values)
            {
                if (node.ParentId.HasValue && dtoMap.TryGetValue(node.ParentId.Value, out var parent))
                {
                    parent.SubModules.Add(node);
                }
                else
                {
                    roots.Add(node);
                }
            }

            // Recursively sort children by SortOrder
            void SortRecursively(List<ModuleResponseDto> list)
            {
                foreach (var item in list)
                {
                    if (item.SubModules != null && item.SubModules.Count > 0)
                    {
                        item.SubModules = item.SubModules.OrderBy(sm => sm.SortOrder).ToList();
                        SortRecursively(item.SubModules);
                    }
                }
            }

            roots = roots.OrderBy(r => r.SortOrder).ToList();
            SortRecursively(roots);

            return roots;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Services.Workflow.Conditions;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;
using KHRMS.Services.Workflow.Rules;
using KHRMS.Services.Workflow.Services;
using Moq;
using Xunit;

namespace KHRMS.UnitTest.ServiceTests
{
    public class WorkflowApproverResolverTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IEmployeeRepository> _mockEmpRepo;
        private readonly Mock<IRoleMasterRepository> _mockRoleRepo;
        private readonly Mock<IEmployeeRoleMappingRepository> _mockRoleMappingRepo;
        private readonly Mock<IProjectMasterRepository> _mockProjRepo;
        private readonly Mock<IDesignationRepository> _mockDesigRepo;
        private readonly Mock<IWorkflowDefinitionRepository> _mockDefRepo;
        private readonly Mock<IWorkflowStepRepository> _mockStepRepo;
        private readonly Mock<IWorkflowRoutingRuleRepository> _mockRuleRepo;
        private readonly Mock<IWorkflowDelegationRepository> _mockDelegationRepo;

        private readonly ConditionEvaluator _conditionEvaluator;

        public WorkflowApproverResolverTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockEmpRepo = new Mock<IEmployeeRepository>();
            _mockRoleRepo = new Mock<IRoleMasterRepository>();
            _mockRoleMappingRepo = new Mock<IEmployeeRoleMappingRepository>();
            _mockProjRepo = new Mock<IProjectMasterRepository>();
            _mockDesigRepo = new Mock<IDesignationRepository>();
            _mockDefRepo = new Mock<IWorkflowDefinitionRepository>();
            _mockStepRepo = new Mock<IWorkflowStepRepository>();
            _mockRuleRepo = new Mock<IWorkflowRoutingRuleRepository>();
            _mockDelegationRepo = new Mock<IWorkflowDelegationRepository>();

            _mockUow.Setup(u => u.Employees).Returns(_mockEmpRepo.Object);
            _mockUow.Setup(u => u.RoleMaster).Returns(_mockRoleRepo.Object);
            _mockUow.Setup(u => u.EmployeeRoleMappings).Returns(_mockRoleMappingRepo.Object);
            _mockUow.Setup(u => u.ProjectMasters).Returns(_mockProjRepo.Object);
            _mockUow.Setup(u => u.Designations).Returns(_mockDesigRepo.Object);
            _mockUow.Setup(u => u.WorkflowDefinitions).Returns(_mockDefRepo.Object);
            _mockUow.Setup(u => u.WorkflowSteps).Returns(_mockStepRepo.Object);
            _mockUow.Setup(u => u.WorkflowRoutingRules).Returns(_mockRuleRepo.Object);
            _mockUow.Setup(u => u.WorkflowDelegations).Returns(_mockDelegationRepo.Object);

            _conditionEvaluator = new ConditionEvaluator();
        }

        // ==========================================
        // Condition Evaluator Tests
        // ==========================================

        [Fact]
        public void ConditionEvaluator_ShouldEvaluateNumericGreaterThan_Correctly()
        {
            var context = new WorkflowContext();
            context.Attributes["Days"] = 6;

            string conditionTrue = "{\"field\":\"Days\",\"operator\":\"GreaterThan\",\"value\":5}";
            string conditionFalse = "{\"field\":\"Days\",\"operator\":\"GreaterThan\",\"value\":10}";

            Assert.True(_conditionEvaluator.Evaluate(conditionTrue, context));
            Assert.False(_conditionEvaluator.Evaluate(conditionFalse, context));
        }

        [Fact]
        public void ConditionEvaluator_ShouldEvaluateStringEquality_Correctly()
        {
            var context = new WorkflowContext { Department = "Engineering" };

            string condTrue = "{\"field\":\"Department\",\"operator\":\"Equal\",\"value\":\"Engineering\"}";
            string condFalse = "{\"field\":\"Department\",\"operator\":\"Equal\",\"value\":\"HR\"}";

            Assert.True(_conditionEvaluator.Evaluate(condTrue, context));
            Assert.False(_conditionEvaluator.Evaluate(condFalse, context));
        }

        [Fact]
        public void ConditionEvaluator_ShouldHandleEmptyCondition_AsTrue()
        {
            var context = new WorkflowContext();
            Assert.True(_conditionEvaluator.Evaluate(null, context));
            Assert.True(_conditionEvaluator.Evaluate("", context));
        }

        // ==========================================
        // Reporting Manager Rule Tests
        // ==========================================

        [Fact]
        public async Task ReportingManagerRule_ShouldReturnDirectManager()
        {
            var employees = new List<Employee>
            {
                new() { Id = 1, FirstName = "Alice", ManagerId = 2, IsActive = true, IsDeleted = false },
                new() { Id = 2, FirstName = "Bob", ManagerId = 3, IsActive = true, IsDeleted = false }
            };
            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(employees);

            var rule = new ReportingManagerRule(_mockUow.Object);
            var result = await rule.ResolveCandidatesAsync(1, new WorkflowContext(), null);

            Assert.Single(result);
            Assert.Equal(2, result[0].EmployeeId);
            Assert.Equal("Bob", result[0].EmployeeName);
        }

        [Fact]
        public async Task ReportingManagerRule_WhenRequesterIsTheirOwnManager_ShouldMoveUpChain()
        {
            // Employee 1 lists themselves (1) as manager, but level 2 is Bob (2)
            var employees = new List<Employee>
            {
                new() { Id = 1, FirstName = "Alice", ManagerId = 1, IsActive = true, IsDeleted = false },
                new() { Id = 2, FirstName = "Bob", ManagerId = null, IsActive = true, IsDeleted = false }
            };
            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(employees);

            var rule = new ReportingManagerRule(_mockUow.Object);
            // Self-referential loop with no other manager returns empty candidate, so resolver triggers fallback chain
            var result = await rule.ResolveCandidatesAsync(1, new WorkflowContext(), null);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ReportingManagerRule_WhenManagerIsInactive_ShouldMoveUpToNextActiveManager()
        {
            var employees = new List<Employee>
            {
                new() { Id = 1, FirstName = "Alice", ManagerId = 2, IsActive = true, IsDeleted = false },
                new() { Id = 2, FirstName = "Bob (Inactive)", ManagerId = 3, IsActive = false, IsDeleted = false },
                new() { Id = 3, FirstName = "Charlie (Active Director)", ManagerId = null, IsActive = true, IsDeleted = false }
            };
            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(employees);

            var rule = new ReportingManagerRule(_mockUow.Object);
            var result = await rule.ResolveCandidatesAsync(1, new WorkflowContext(), null);

            Assert.Single(result);
            Assert.Equal(3, result[0].EmployeeId);
            Assert.Equal("Charlie (Active Director)", result[0].EmployeeName);
        }

        // ==========================================
        // Skip-Level Manager Rule Tests
        // ==========================================

        [Fact]
        public async Task SkipLevelManagerRule_ShouldReturnManagersManager()
        {
            var employees = new List<Employee>
            {
                new() { Id = 1, FirstName = "Alice", ManagerId = 2, IsActive = true, IsDeleted = false },
                new() { Id = 2, FirstName = "Bob", ManagerId = 3, IsActive = true, IsDeleted = false },
                new() { Id = 3, FirstName = "Charlie", ManagerId = null, IsActive = true, IsDeleted = false }
            };
            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(employees);

            var rule = new SkipLevelManagerRule(_mockUow.Object);
            var result = await rule.ResolveCandidatesAsync(1, new WorkflowContext(), null);

            Assert.Single(result);
            Assert.Equal(3, result[0].EmployeeId);
            Assert.Equal("Charlie", result[0].EmployeeName);
        }

        // ==========================================
        // Department Head Rule Tests
        // ==========================================

        [Fact]
        public async Task DepartmentHeadRule_ShouldFindHeadByDesignationKeyword()
        {
            var designations = new List<Designation>
            {
                new() { Id = 10, DepartmentCategory = "Engineering", DesignationName = "Software Engineer" },
                new() { Id = 20, DepartmentCategory = "Engineering", DesignationName = "Engineering Director / Head" }
            };
            var employees = new List<Employee>
            {
                new() { Id = 1, FirstName = "Alice", DesignationId = 10, IsActive = true, IsDeleted = false },
                new() { Id = 5, FirstName = "Dan", DesignationId = 20, IsActive = true, IsDeleted = false }
            };

            _mockDesigRepo.Setup(r => r.GetAll()).ReturnsAsync(designations);
            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(employees);

            var rule = new DepartmentHeadRule(_mockUow.Object);
            var context = new WorkflowContext { Department = "Engineering" };
            var result = await rule.ResolveCandidatesAsync(1, context, null);

            Assert.Single(result);
            Assert.Equal(5, result[0].EmployeeId);
            Assert.Equal("Dan", result[0].EmployeeName);
        }

        // ==========================================
        // Role In Location Rule Tests
        // ==========================================

        [Fact]
        public async Task RoleInLocationRule_ShouldMatchRoleAndBranch()
        {
            var roles = new List<RoleMaster>
            {
                new() { Id = 2, RoleName = "HR Operations", IsActive = true, IsDeleted = false }
            };
            var roleMappings = new List<EmployeeRoleMapping>
            {
                new() { EmployeeId = 10, RoleId = 2, IsActive = true, IsDeleted = false },
                new() { EmployeeId = 11, RoleId = 2, IsActive = true, IsDeleted = false }
            };
            var employees = new List<Employee>
            {
                new() { Id = 1, FirstName = "Alice", Branch = "Bangalore", IsActive = true, IsDeleted = false },
                new() { Id = 10, FirstName = "HR Bangalore", Branch = "Bangalore", IsActive = true, IsDeleted = false },
                new() { Id = 11, FirstName = "HR HQ", Branch = "HQ", IsActive = true, IsDeleted = false }
            };

            _mockRoleRepo.Setup(r => r.GetAll()).ReturnsAsync(roles);
            _mockRoleMappingRepo.Setup(r => r.GetAll()).ReturnsAsync(roleMappings);
            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(employees);

            var rule = new RoleInLocationRule(_mockUow.Object);
            var context = new WorkflowContext { Location = "Bangalore" };
            var result = await rule.ResolveCandidatesAsync(1, context, "{\"RoleName\":\"HR Operations\"}");

            Assert.Single(result);
            Assert.Equal(10, result[0].EmployeeId);
        }

        // ==========================================
        // Project Owner Rule Tests
        // ==========================================

        [Fact]
        public async Task ProjectOwnerRule_ShouldReturnProjectManager()
        {
            var projects = new List<ProjectMaster>
            {
                new() { Id = 100, ProjectName = "Cloud Migration", ProjectManagerId = 42, IsActive = true, IsDeleted = false }
            };
            var employees = new List<Employee>
            {
                new() { Id = 1, FirstName = "Alice", IsActive = true, IsDeleted = false },
                new() { Id = 42, FirstName = "Project Manager Dave", IsActive = true, IsDeleted = false }
            };

            _mockProjRepo.Setup(r => r.GetAll()).ReturnsAsync(projects);
            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(employees);

            var rule = new ProjectOwnerRule(_mockUow.Object);
            var context = new WorkflowContext { ProjectId = 100 };
            var result = await rule.ResolveCandidatesAsync(1, context, null);

            Assert.Single(result);
            Assert.Equal(42, result[0].EmployeeId);
        }

        // ==========================================
        // Specific Person & Group Queue Rules Tests
        // ==========================================

        [Fact]
        public async Task SpecificPersonRule_ShouldReturnTargetEmployee()
        {
            var employees = new List<Employee>
            {
                new() { Id = 1, FirstName = "Alice", IsActive = true, IsDeleted = false },
                new() { Id = 77, FirstName = "Specific Approver Sam", IsActive = true, IsDeleted = false }
            };
            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(employees);

            var rule = new SpecificPersonRule(_mockUow.Object);
            var result = await rule.ResolveCandidatesAsync(1, new WorkflowContext(), "{\"EmployeeId\":77}");

            Assert.Single(result);
            Assert.Equal(77, result[0].EmployeeId);
        }

        [Fact]
        public async Task GroupQueueRule_ShouldReturnAllRoleMembersExceptRequester()
        {
            var roles = new List<RoleMaster>
            {
                new() { Id = 3, RoleName = "Finance", IsActive = true, IsDeleted = false }
            };
            var roleMappings = new List<EmployeeRoleMapping>
            {
                new() { EmployeeId = 1, RoleId = 3, IsActive = true, IsDeleted = false }, // Requester!
                new() { EmployeeId = 21, RoleId = 3, IsActive = true, IsDeleted = false },
                new() { EmployeeId = 22, RoleId = 3, IsActive = true, IsDeleted = false }
            };
            var employees = new List<Employee>
            {
                new() { Id = 1, FirstName = "Alice", IsActive = true, IsDeleted = false },
                new() { Id = 21, FirstName = "Fin1", IsActive = true, IsDeleted = false },
                new() { Id = 22, FirstName = "Fin2", IsActive = true, IsDeleted = false }
            };

            _mockRoleRepo.Setup(r => r.GetAll()).ReturnsAsync(roles);
            _mockRoleMappingRepo.Setup(r => r.GetAll()).ReturnsAsync(roleMappings);
            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(employees);

            var rule = new GroupQueueRule(_mockUow.Object);
            var result = await rule.ResolveCandidatesAsync(1, new WorkflowContext(), "{\"RoleName\":\"Finance\"}");

            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, r => r.EmployeeId == 1); // Requester excluded
        }

        // ==========================================
        // Resolver: Routing Rules & Priority Tests
        // ==========================================

        [Fact]
        public async Task ApproverResolver_ShouldPickLowestPriorityRoutingRule()
        {
            var routingRules = new List<WorkflowRoutingRule>
            {
                new() { Id = 1, RequestType = "Leave", ScopeType = "Default", WorkflowDefinitionId = 10, Priority = 999, IsActive = true, IsDeleted = false },
                new() { Id = 2, RequestType = "Leave", ScopeType = "Department", ScopeValue = "Engineering", WorkflowDefinitionId = 20, Priority = 10, IsActive = true, IsDeleted = false }
            };
            var def20 = new WorkflowDefinition { Id = 20, RequestType = "Leave", Title = "Engineering Leave Flow", Version = 1, IsActive = true, IsDeleted = false };

            _mockRuleRepo.Setup(r => r.GetAll()).ReturnsAsync(routingRules);
            _mockDefRepo.Setup(r => r.GetById(20)).ReturnsAsync(def20);
            _mockStepRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowStep>());

            var rules = new List<IApproverRule>();
            var resolver = new ApproverResolver(_mockUow.Object, rules, _conditionEvaluator);

            var context = new WorkflowContext { Department = "Engineering" };
            var selectedDef = await resolver.ResolveDefinitionAsync("Leave", context);

            Assert.NotNull(selectedDef);
            Assert.Equal(20, selectedDef.Id);
            Assert.Equal("Engineering Leave Flow", selectedDef.Title);
        }

        [Fact]
        public async Task ApproverResolver_ShouldFallbackToDefaultRoutingRule_WhenNoSpecificScopeMatches()
        {
            var routingRules = new List<WorkflowRoutingRule>
            {
                new() { Id = 1, RequestType = "Leave", ScopeType = "Default", WorkflowDefinitionId = 10, Priority = 999, IsActive = true, IsDeleted = false },
                new() { Id = 2, RequestType = "Leave", ScopeType = "Department", ScopeValue = "Sales", WorkflowDefinitionId = 30, Priority = 10, IsActive = true, IsDeleted = false }
            };
            var def10 = new WorkflowDefinition { Id = 10, RequestType = "Leave", Title = "Default Leave Flow", Version = 1, IsActive = true, IsDeleted = false };

            _mockRuleRepo.Setup(r => r.GetAll()).ReturnsAsync(routingRules);
            _mockDefRepo.Setup(r => r.GetById(10)).ReturnsAsync(def10);
            _mockStepRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowStep>());

            var rules = new List<IApproverRule>();
            var resolver = new ApproverResolver(_mockUow.Object, rules, _conditionEvaluator);

            // Context has Department = "Engineering", so Sales rule doesn't match; should fallback to Default (def 10)
            var context = new WorkflowContext { Department = "Engineering" };
            var selectedDef = await resolver.ResolveDefinitionAsync("Leave", context);

            Assert.NotNull(selectedDef);
            Assert.Equal(10, selectedDef.Id);
            Assert.Equal("Default Leave Flow", selectedDef.Title);
        }

        // ==========================================
        // Resolver: Delegation & Fallback Tests
        // ==========================================

        [Fact]
        public async Task ApproverResolver_ShouldApplyActiveDelegation()
        {
            var employees = new List<Employee>
            {
                new() { Id = 1, FirstName = "Requester Alice", ManagerId = 2, IsActive = true, IsDeleted = false },
                new() { Id = 2, FirstName = "Manager Bob", ManagerId = null, IsActive = true, IsDeleted = false },
                new() { Id = 99, FirstName = "Delegate Dave", ManagerId = null, IsActive = true, IsDeleted = false }
            };
            var delegations = new List<WorkflowDelegation>
            {
                new()
                {
                    Id = 1,
                    FromEmployeeId = 2,
                    ToEmployeeId = 99,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(5),
                    RequestType = "Leave",
                    IsActive = true,
                    IsDeleted = false
                }
            };
            var definition = new WorkflowDefinition
            {
                Id = 1,
                RequestType = "Leave",
                Title = "Leave Flow",
                Steps = new List<WorkflowStep>
                {
                    new() { Id = 1, WorkflowDefinitionId = 1, StepOrder = 1, StepName = "Manager Approval", ApproverRuleType = "ReportingManager", StepMode = "AnyOne", IsActive = true, IsDeleted = false }
                }
            };

            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(employees);
            _mockDelegationRepo.Setup(r => r.GetAll()).ReturnsAsync(delegations);

            var reportingRule = new ReportingManagerRule(_mockUow.Object);
            var resolver = new ApproverResolver(_mockUow.Object, new[] { reportingRule }, _conditionEvaluator);

            var resolvedSteps = await resolver.ResolveStepsAsync("Leave", 1, new WorkflowContext(), definition);

            Assert.Single(resolvedSteps);
            var approvers = resolvedSteps[0].Approvers;
            Assert.Equal(2, approvers.Count);

            var principal = approvers.FirstOrDefault(a => a.EmployeeId == 2);
            var delegateUser = approvers.FirstOrDefault(a => a.EmployeeId == 99);

            Assert.NotNull(principal);
            Assert.Equal("Approver", principal.Capacity);

            Assert.NotNull(delegateUser);
            Assert.Equal("Delegate", delegateUser.Capacity);
            Assert.Equal(2, delegateUser.DelegatedFromEmployeeId);
        }

        [Fact]
        public async Task ApproverResolver_WhenNoApproversFoundAndFallbackExhausted_ShouldThrowClearException()
        {
            // Orphaned employee with no manager, no HR/Admin in system
            var employees = new List<Employee>
            {
                new() { Id = 1, FirstName = "Lone Employee", ManagerId = null, IsActive = true, IsDeleted = false }
            };
            var definition = new WorkflowDefinition
            {
                Id = 1,
                RequestType = "Leave",
                Title = "Leave Flow",
                Steps = new List<WorkflowStep>
                {
                    new() { Id = 1, WorkflowDefinitionId = 1, StepOrder = 1, StepName = "Manager Approval", ApproverRuleType = "ReportingManager", StepMode = "AnyOne", IsActive = true, IsDeleted = false }
                }
            };

            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(employees);
            _mockDelegationRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowDelegation>());
            _mockRoleRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<RoleMaster>());
            _mockRoleMappingRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<EmployeeRoleMapping>());

            var reportingRule = new ReportingManagerRule(_mockUow.Object);
            var resolver = new ApproverResolver(_mockUow.Object, new[] { reportingRule }, _conditionEvaluator);

            var ex = await Assert.ThrowsAsync<WorkflowResolutionException>(() =>
                resolver.ResolveStepsAsync("Leave", 1, new WorkflowContext(), definition));

            Assert.Contains("could not resolve any eligible approvers", ex.Message);
            Assert.Contains("fallback chain was exhausted", ex.Message);
        }
    }
}

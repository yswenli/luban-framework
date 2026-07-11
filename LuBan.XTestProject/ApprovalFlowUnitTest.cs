using LuBan.ApprovalFlow.Consts;
using LuBan.ApprovalFlow.Core;
using LuBan.ApprovalFlow.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LuBan.XTestProject;

[TestClass]
public class ApprovalFlowUnitTest
{
    [TestMethod]
    public void AggregationEvaluator_AllApprove_ShouldReturnApproved_WhenAllApproved()
    {
        var evaluator = new AggregationEvaluator();
        
        var result = evaluator.Evaluate(ConstAggregationType.AllApprove, 3, 0, 3);
        
        Assert.AreEqual(AggregationResult.Approved, result);
    }

    [TestMethod]
    public void AggregationEvaluator_AllApprove_ShouldReturnRejected_WhenAnyRejected()
    {
        var evaluator = new AggregationEvaluator();
        
        var result = evaluator.Evaluate(ConstAggregationType.AllApprove, 2, 1, 3);
        
        Assert.AreEqual(AggregationResult.Rejected, result);
    }

    [TestMethod]
    public void AggregationEvaluator_AllApprove_ShouldReturnPending_WhenNotComplete()
    {
        var evaluator = new AggregationEvaluator();
        
        var result = evaluator.Evaluate(ConstAggregationType.AllApprove, 2, 0, 3);
        
        Assert.AreEqual(AggregationResult.Pending, result);
    }

    [TestMethod]
    public void AggregationEvaluator_AnyApprove_ShouldReturnApproved_WhenAnyApproved()
    {
        var evaluator = new AggregationEvaluator();
        
        var result = evaluator.Evaluate(ConstAggregationType.AnyApprove, 1, 0, 3);
        
        Assert.AreEqual(AggregationResult.Approved, result);
    }

    [TestMethod]
    public void AggregationEvaluator_MajorityApprove_ShouldReturnApproved_WhenMajorityApproved()
    {
        var evaluator = new AggregationEvaluator();
        
        var result = evaluator.Evaluate(ConstAggregationType.MajorityApprove, 2, 1, 3);
        
        Assert.AreEqual(AggregationResult.Approved, result);
    }

    [TestMethod]
    public void AggregationEvaluator_PercentageApprove_ShouldReturnApproved_WhenPercentageReached()
    {
        var evaluator = new AggregationEvaluator();
        
        var result = evaluator.Evaluate(ConstAggregationType.PercentageApprove, 6, 2, 10, 60);
        
        Assert.AreEqual(AggregationResult.Approved, result);
    }

    [TestMethod]
    public void RuleEngine_Evaluate_ShouldMatchRule_WhenConditionMet()
    {
        var engine = new RuleEngine();
        
        var rules = new List<GatewayRule>
        {
            new GatewayRule
            {
                EdgeId = "edge-1",
                EdgeText = "通过",
                Conditions = new List<RuleCondition>
                {
                    new RuleCondition { Field = "amount", Operator = ConstOperatorType.GreaterThan, Value = 50000 }
                },
                Logic = "and"
            }
        };
        
        var variables = new Dictionary<string, object> { { "amount", 60000 } };
        
        var result = engine.Evaluate(rules, variables, "default-edge");
        
        Assert.IsTrue(result.Matched);
        Assert.AreEqual("edge-1", result.EdgeId);
    }

    [TestMethod]
    public void RuleEngine_Evaluate_ShouldReturnDefault_WhenNoRuleMatches()
    {
        var engine = new RuleEngine();
        
        var rules = new List<GatewayRule>
        {
            new GatewayRule
            {
                EdgeId = "edge-1",
                EdgeText = "通过",
                Conditions = new List<RuleCondition>
                {
                    new RuleCondition { Field = "amount", Operator = ConstOperatorType.GreaterThan, Value = 50000 }
                },
                Logic = "and"
            }
        };
        
        var variables = new Dictionary<string, object> { { "amount", 10000 } };
        
        var result = engine.Evaluate(rules, variables, "default-edge");
        
        Assert.IsFalse(result.Matched);
        Assert.AreEqual("default-edge", result.EdgeId);
    }

    [TestMethod]
    public void RuleEngine_EvaluateCondition_ShouldReturnTrue_ForEqualOperator()
    {
        var engine = new RuleEngine();
        
        var condition = new RuleCondition { Field = "status", Operator = ConstOperatorType.Equal, Value = "approved" };
        var variables = new Dictionary<string, object> { { "status", "approved" } };
        
        var result = engine.EvaluateCondition(condition, variables);
        
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void RuleEngine_EvaluateCondition_ShouldReturnTrue_ForLessThanOperator()
    {
        var engine = new RuleEngine();
        
        var condition = new RuleCondition { Field = "amount", Operator = ConstOperatorType.LessThan, Value = 10000 };
        var variables = new Dictionary<string, object> { { "amount", 5000 } };
        
        var result = engine.EvaluateCondition(condition, variables);
        
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void PlaceholderResolver_Resolve_ShouldReplaceVariables()
    {
        var context = new FlowExecutionContext
        {
            RecordId = 1001,
            FlowName = "采购审批",
            Variables = new Dictionary<string, object> { { "amount", 50000 } }
        };
        
        var template = "流程${flowName}，记录ID=${recordId}，金额=${variables.amount}";
        
        var result = PlaceholderResolver.Resolve(template, context);
        
        Assert.AreEqual("流程采购审批，记录ID=1001，金额=50000", result);
    }

    [TestMethod]
    public void PlaceholderResolver_ResolveDictionary_ShouldReplaceAllVariables()
    {
        var context = new FlowExecutionContext
        {
            RecordId = 1001,
            Variables = new Dictionary<string, object> { { "userId", 100 } }
        };
        
        var dict = new Dictionary<string, object>
        {
            { "url", "https://api.example.com/${recordId}" },
            { "body", new Dictionary<string, object> { { "userId", "${variables.userId}" } } }
        };
        
        var result = PlaceholderResolver.ResolveDictionary(dict, context);
        
        Assert.AreEqual("https://api.example.com/1001", result["url"]);
        var bodyDict = result["body"] as Dictionary<string, object>;
        Assert.AreEqual("100", bodyDict?["userId"]);
    }
}
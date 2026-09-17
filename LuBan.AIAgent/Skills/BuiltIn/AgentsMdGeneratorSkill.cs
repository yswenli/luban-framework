/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Skills.BuiltIn
*文件名： AgentsMdGeneratorSkill
*版本号： V1.0.0.0
*唯一标识：新建
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/17
*描述：Agents.md 规约生成器内置 Skill，生成工程化 Agent 元描述文档 AGENTS.md
*
*****************************************************************************/
namespace LuBan.AIAgent.Skills.BuiltIn;

/// <summary>
/// Agents.md 规约生成器 Skill：结合工作区实际结构生成工程化 Agent 元描述文档 AGENTS.md，
/// 定义 Agent 角色、能力、工具、权限、边界与异常策略
/// </summary>
public class AgentsMdGeneratorSkill : SkillBase
{
    /// <summary>
    /// Skill 唯一标识
    /// </summary>
    public override string Id => "agents-md-generator";
    /// <summary>
    /// Skill 名称
    /// </summary>
    public override string Name => "Agents.md 规约生成器";
    /// <summary>
    /// Skill 描述
    /// </summary>
    public override string Description => "生成工程化Agent元描述文档 AGENTS.md，定义Agent角色、能力、工具、权限、边界与异常策略，供Agent调度框架、LLM读取";
    /// <summary>
    /// Skill 分类
    /// </summary>
    public override string Category => "productivity";

    /// <summary>
    /// Skill 使用示例
    /// </summary>
    public override IEnumerable<string> Examples => new[]
    {
        "生成agents.md",
        "写agent规约",
        "agent能力描述文档"
    };

    /// <summary>
    /// Skill 自动激活触发关键词
    /// </summary>
    public override IEnumerable<string> TriggerKeywords => new[]
    {
        "生成agents.md",
        "写agent规约",
        "agent能力描述文档",
        "agents.md",
        "初始化工作区"
    };

    /// <summary>
    /// Skill 的提示词模板内容
    /// </summary>
    public override string PromptTemplate => @"你是一个 Agent 元描述文档（AGENTS.md）生成专家。请根据调用方提供的参数与工作区实际结构，生成工程化的 AGENTS.md 文档。

## 输入参数

调用方会以 `key: value` 形式提供以下参数，未提供的参数必须从工作区实际结构推断，禁止编造：

- agent_name：Agent 名称（必填）
- agent_scene：业务场景（必填）
- capabilities：能力清单，逗号分隔（选填）
- tools：工具/API 清单，逗号分隔（选填）
- output_mode：输出模式，full 或 lite，默认 full
- description：补充说明（选填）
- existing_agents_md：工作区已存在的 AGENTS.md 全文（选填）

## 生成前必须执行的动作

1. 浏览工作区结构，识别编程语言、框架、构建与测试命令、目录约定、依赖清单。
2. 若输入包含 existing_agents_md，采用合并更新策略：保留仍然正确的内容，补充缺失章节，修正与实际不符的描述，不得整篇重写丢弃原有有效信息。
3. 所有技术细节（命令、路径、工具名）必须来自工作区真实内容。

## 输出模式

### full 模式（默认）：固定 10 个章节，顺序不可调整

1. **Metadata**：版本、更新时间、适用范围、文档用途
2. **角色定位**：Agent 的职责、目标、服务对象
3. **业务场景**：典型使用场景与触发条件
4. **能力清单**：必须使用 Markdown 表格，列为：能力 | 说明 | 触发方式
5. **工具与API**：必须使用 Markdown 表格，列为：工具/API | 用途 | 调用方式 | 限制
6. **权限与授权**：可访问资源、写操作范围、需要审批的操作
7. **禁止行为**：明确禁止的操作，必须独立成章
8. **异常处理**：错误分类、重试策略、降级方案、上报方式，必须独立成章
9. **输入输出规约**：输入格式、输出格式、命名约定、编码要求
10. **协作与调度接口**：与其它 Agent 或系统的交互协议、调度入口、状态约定

### lite 模式：固定 4 个章节

1. **Metadata**
2. **能力清单**（Markdown 表格）
3. **工具与API**（Markdown 表格）
4. **边界**：合并「禁止行为」与「异常处理」

## 写作要求

- 纯技术文档风格，不写抒情、鼓励、寒暄类文字。
- 章节标题保持上述中文名称原文，不要翻译、不要改写。
- full 模式下「禁止行为」与「异常处理」必须独立成章，不得并入其它章节。

## 输出契约（最高优先级）

- 只输出 AGENTS.md 正文 Markdown 本身，第一个字符必须是 `#`。
- 禁止使用 ``` 代码围栏包裹全文。
- 禁止输出「以下是生成结果」「希望对你有帮助」等任何前后缀或说明文字。

## 调用方输入

{input}";
}
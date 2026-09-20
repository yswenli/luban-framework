/****************************************************************************
*Copyright @ yswenli All Rights Reserved.
*CLR版本： .net10.0
*机器名称：WALLE
*公司名称：Walle
*命名空间：LuBan.AIAgent.Wiki
*文件名： WikiDefaults
*版本号： V1.0.0.0
*唯一标识：c41f8a37-5e92-4b06-9d17-2f8e6a0b7c44
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2026/9/20
*描述：wiki 默认 schema 文案（承载 LLM 创作规范）
*
*****************************************************************************/
namespace LuBan.AIAgent.Wiki;

/// <summary>wiki 默认 schema 文案（承载 LLM 创作规范）。</summary>
public static class WikiDefaults
{
    /// <summary>默认 schema：目录布局、frontmatter、链接、index/log 约定。</summary>
    public const string Schema = """
        ## LLM Wiki 维护规范

        你正在维护工作区的 `wiki/` 知识库。规则：
        1. 目录：`sources/`（来源摘要，一源一页）、`entities/`（实体）、`concepts/`（概念）、`queries/`（问答沉淀）；根目录保留 `index.md`、`log.md`、`overview.md`。
        2. 每个页面以 YAML frontmatter 开头，必须包含：`title`、`type`（source|entity|concept|overview|query）、`tags`、`sources`（相对工作区根的来源路径）、`created`、`updated`。
        3. 页面之间用**相对 markdown 链接**互链（如 `../entities/张三.md`）。
        4. 写入/删除页面**必须**调用 `wiki.savePage` / `wiki.deletePage`，由服务负责 index.md、log.md 与向量索引；不要手写 index.md 或 log.md。
        5. 新增内容前先 `wiki.readIndex` 了解已有页面，避免重复建页；发现相关内容应优先并入既有页面。
        6. 回答用户前先 `wiki.search`；wiki 无相关内容时再考虑回落 raw（`includeRaw=true`）。
        7. 定期可用 `wiki.lint` 检查孤儿页、死链、未收录、来源缺失。
        """;
}
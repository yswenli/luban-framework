/****************************************************************************
*Copyright (c) YSWenli All Rights Reserved.
*CLR版本： .net8.0
*机器名称：WALLE
*Author：yswenli
*命名空间：LuBan.Common.Data
*文件名： CurrencyUtil
*版本号： V1.0.0.0
*唯一标识：2b1970dd-3e9e-4f78-8af8-979da2e6809b
*当前的用户域：WALLE
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2025/8/7 15:19:59
*描述：货币工具类
*
*=================================================
*修改标记
*修改时间：2025/8/7 15:19:59
*修改人： yswenli
*版本号： V1.0.0.0
*描述：货币工具类
*
*****************************************************************************/

namespace LuBan.Common.Data;

/// <summary>
/// 货币工具类
/// </summary>
public static class CurrencyUtil
{
    /// <summary>
    /// 将金额转换为中文大写
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string ConvertToChinese(this decimal amount)
    {
        if (amount < 0) { throw new ArgumentOutOfRangeException(nameof(amount), "金额不能为负数"); }

        return ChineseCurrencyConverter.ConvertToChinese(amount);
    }
}
/// <summary>
/// 中文货币转换器
/// </summary>
public static class ChineseCurrencyConverter
{
    // 数字对应的中文大写
    private static readonly char[] _digits = { '零', '壹', '贰', '叁', '肆', '伍', '陆', '柒', '捌', '玖' };
    // 整数部分单位（从低位到高位）
    private static readonly string[] _intUnits = { "元", "拾", "佰", "仟", "万", "拾", "佰", "仟", "亿", "拾", "佰", "仟", "兆" };
    // 小数部分单位
    private static readonly string[] _decimalUnits = { "角", "分" };

    /// <summary>
    /// 将金额转换为中文大写
    /// </summary>
    /// <param name="amount">金额（支持正负，小数部分最多保留两位）</param>
    /// <returns>中文大写金额字符串</returns>
    /// <exception cref="ArgumentOutOfRangeException">金额超出范围时抛出</exception>
    public static string ConvertToChinese(decimal amount)
    {
        // 处理负数
        string negativeSign = "";
        if (amount < 0)
        {
            negativeSign = "负";
            amount = -amount;
        }

        // 限制金额范围（根据实际需求调整，这里限制为千亿级）
        if (amount > 9999999999999.99m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "金额过大，超出转换范围");
        }

        // 拆分整数和小数部分（四舍五入保留两位小数）
        decimal rounded = Math.Round(amount, 2);
        long integerPart = (long)rounded;
        int decimalPart = (int)Math.Round((rounded - integerPart) * 100);

        // 转换整数部分
        string integerStr = ConvertIntegerPart(integerPart);
        // 转换小数部分
        string decimalStr = ConvertDecimalPart(decimalPart);

        // 组合结果（加上负数符号）
        return negativeSign + integerStr + decimalStr;
    }

    /// <summary>
    /// 转换整数部分
    /// </summary>
    /// <param name="integerPart"></param>
    /// <returns></returns>
    private static string ConvertIntegerPart(long integerPart)
    {
        if (integerPart == 0)
        {
            return "零元"; // 整数部分为0时，必须保留"零元"
        }

        char[] digits = integerPart.ToString().ToCharArray();
        int digitCount = digits.Length;
        string result = "";
        bool hasZero = false; // 标记是否有未处理的零

        for (int i = 0; i < digitCount; i++)
        {
            int digit = digits[i] - '0';
            int unitIndex = digitCount - 1 - i; // 单位索引（从0开始，对应"元"的位置）

            if (digit == 0)
            {
                hasZero = true;
            }
            else
            {
                // 如果之前有零，先补一个"零"
                if (hasZero)
                {
                    result += "零";
                    hasZero = false;
                }
                // 拼接数字和单位
                result += _digits[digit] + _intUnits[unitIndex];
            }

            // 处理单位分级（万、亿等）：即使该位为0，单位仍需保留（如"壹万零仟零佰"简化为"壹万"）
            if (unitIndex % 4 == 0 && unitIndex > 0) // 万（4）、亿（8）、兆（12）等位置
            {
                if (result.EndsWith(_intUnits[unitIndex]) == false)
                {
                    result += _intUnits[unitIndex];
                }
                hasZero = false; // 分级后重置零标记
            }
        }

        return result;
    }

    /// <summary>
    /// 转换小数部分（角和分）
    /// </summary>
    /// <param name="decimalPart"></param>
    /// <returns></returns>
    private static string ConvertDecimalPart(int decimalPart)
    {
        if (decimalPart == 0)
        {
            return "元整"; // 无小数部分时加"整"
        }

        int jiao = decimalPart / 10; // 角
        int fen = decimalPart % 10;  // 分

        string jiaoStr = jiao > 0 ? _digits[jiao] + _decimalUnits[0] : "";
        string fenStr = fen > 0 ? _digits[fen] + _decimalUnits[1] : "";

        return jiaoStr + fenStr;
    }
}
using SmartEcommerce.BuildingBlock.Domain.Attributes;
using SmartEcommerce.BuildingBlock.Domain.Enums;

namespace SmartEcommerce.BuildingBlock.Domain.Extensions;

public static class MessageCodeExtension
{
    public static string ToMessage(this MessageCode code)
        => MessageCodeAttribute.GetMessage(code);

    public static string ToStringCode(this MessageCode code)
        => MessageCodeAttribute.GetCode(code);

    public static string ToValue(this MessageCode code)
        => ((int)code).ToString();
}

using MessageCodeEnum = SmartEcommerce.BuildingBlock.Domain.Enums.MessageCode;

namespace SmartEcommerce.BuildingBlock.Application.Exceptions;

public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string? systemMessage = null)
        : base(MessageCodeEnum.Forbidden, systemMessage, statusCode: 403) { }

    public ForbiddenException(MessageCodeEnum messageCode, string? systemMessage = null)
        : base(messageCode, systemMessage, statusCode: 403) { }
}

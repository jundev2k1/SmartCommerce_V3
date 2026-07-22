using MessageCodeEnum = BuildingBlock.Domain.Enums.MessageCode;

namespace BuildingBlock.Application.Exceptions;

public class ConflictException : ApplicationException
{
    public ConflictException(string? systemMessage = null)
        : base(MessageCodeEnum.Conflict, systemMessage, statusCode: 409) { }

    public ConflictException(MessageCodeEnum messageCode, string? systemMessage = null)
        : base(messageCode, systemMessage, statusCode: 409) { }
}

using BuildingBlock.Application.Exceptions;

using Product.Application.Abstractions.Repositories;
using Product.Domain.ValueObjects;

namespace Product.Application.Features.ProductTags.Commands.CreateProductTag;

public sealed class CreateProductTagHandler(
    IProductTagRepository tagRepo,
    IUnitOfWork uow) : ICommandHandler<CreateProductTagCommand, CreateProductTagResponse>
{
    public async Task<CreateProductTagResponse> Handle(CreateProductTagCommand request, CancellationToken ct = default)
    {
        if (await tagRepo.CodeExistsAsync(request.Code, ct))
            throw new ConflictException($"ProductTag with code ({request.Code}) already exists");

        var tag = ProductTag.Create(Guid.CreateVersion7(), TagCode.Create(request.Code), request.Name.Trim());
        await tagRepo.AddAsync(tag, ct);
        await uow.SaveChangesAsync(ct);

        return new CreateProductTagResponse(tag.Id);
    }
}

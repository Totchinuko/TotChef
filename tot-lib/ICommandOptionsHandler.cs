using System.Diagnostics.CodeAnalysis;

namespace tot_lib;


public interface ICommandOptionsHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]in TOptions>
{
    Task<int> HandleAsync(TOptions options, CancellationToken cancellationToken);
}
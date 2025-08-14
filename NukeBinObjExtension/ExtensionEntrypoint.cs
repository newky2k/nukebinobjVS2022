using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.Extensibility;

namespace NukeBinObjExtension
{
    /// <summary>
    /// Extension entrypoint for the VisualStudio.Extensibility extension.
    /// </summary>
    [VisualStudioContribution]
    internal class ExtensionEntrypoint : Extension
    {
        /// <inheritdoc/>
        public override ExtensionConfiguration ExtensionConfiguration => new()
        {
            Metadata = new(
                    id: "NukeBinObjExtension.86019190-4440-4de9-bf7e-590dd3d2c64b",
                    version: this.ExtensionAssemblyVersion,
                    publisherName: "Newky2k",
                    displayName: "Nuke Bin/Obj",
                    description: "Simple extension to fully delete the bin/obj folder of the selected project")
            {
                Preview = false,
            },
        };

        /// <inheritdoc />
        protected override void InitializeServices(IServiceCollection serviceCollection)
        {
            base.InitializeServices(serviceCollection);

            serviceCollection.TryAddTransient<NukeService>();

            // You can configure dependency injection here by adding services to the serviceCollection.
        }
    }
}

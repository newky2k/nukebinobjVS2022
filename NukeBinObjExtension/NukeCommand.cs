using System.Diagnostics;
using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using Microsoft.VisualStudio.ProjectSystem.Query;

namespace NukeBinObjExtension
{
    /// <summary>
    /// Command1 handler.
    /// </summary>
    [VisualStudioContribution]
    internal class NukeCommand : Command
    {
        private readonly TraceSource logger;
        private readonly NukeService _nukeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NukeCommand"/> class.
        /// </summary>
        /// <param name="traceSource">Trace source instance to utilize.</param>
        public NukeCommand(TraceSource traceSource, NukeService nukeService)
        {
            // This optional TraceSource can be used for logging in the command. You can use dependency injection to access
            // other services here as well.
            this.logger = Requires.NotNull(traceSource, nameof(traceSource));

            this._nukeService = nukeService;
        }

        /// <inheritdoc />
        public override CommandConfiguration CommandConfiguration => new("%NukeBinObjExtension.NukeCommand.DisplayName%")
        {
            // Use this object initializer to set optional parameters for the command. The required parameter,
            // displayName, is set above. DisplayName is localized and references an entry in .vsextension\string-resources.json.
            Icon = new(ImageMoniker.KnownValues.CleanData, IconSettings.IconAndText),
            Placements =
            [
                // Project context menu
                CommandPlacement.VsctParent(new Guid("{d309f791-903f-11d0-9efc-00a0c911004f}"), id: 518, priority: 0),
            ],
            EnabledWhen = ActivationConstraint.And(!ActivationConstraint.SolutionState(SolutionState.Building),ActivationConstraint.SolutionState(SolutionState.FullyLoaded)),
        };

        /// <inheritdoc />
        public override Task InitializeAsync(CancellationToken cancellationToken)
        {
            // Use InitializeAsync for any one-time setup or initialization.
            return base.InitializeAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
        {
            var project = await context.GetActiveProjectAsync(project => project.With(p => new { p.Name, p.Path })
                            .With(p => p.ActiveConfigurations),
                            cancellationToken);

            if (project != null)
            {
                var projectName = project.Name;
               

                var result = await this.Extensibility.Shell().ShowPromptAsync($"Are you sure you want to delete the bin/obj folders for {projectName} ?", PromptOptions.OKCancel, cancellationToken);

                if (result)
                {
                    if (project.Path is string projectPath)
                    {
                        await _nukeService.NukeAsync(projectPath);

                        await this.Extensibility.Shell().ShowPromptAsync("The bin/obj folders have been Nuked!", PromptOptions.OK, cancellationToken);
                    }
                    
                }
            }
          
        }
    }
}
